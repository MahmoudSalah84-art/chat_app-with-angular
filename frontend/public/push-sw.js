/* eslint-disable no-restricted-globals */
/**
 * Chatme Service Worker (Web Push فقط، مفيش offline caching).
 * مكانه: frontend/public/push-sw.js  → بيتخدم من الـ root (/push-sw.js) فالـ scope بتاعه "/".
 */

const OPEN_CHAT_MESSAGE = 'chatme:open-chat';
const SUBSCRIPTION_CHANGED_MESSAGE = 'chatme:subscription-changed';
const DEFAULT_ICON = '/icons/icon-192.png';
const MAX_BODY_LINES = 5; // آخر كام رسالة نجمّعهم في notification واحدة لنفس الشات

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()));

// ─────────────────────────── Push ───────────────────────────

self.addEventListener('push', (event) => {
  event.waitUntil(handlePush(event));
});

async function handlePush(event) {
  const data = parsePayload(event);

  // لو التطبيق ظاهر قدام المستخدم، الرسالة وصلته بالفعل عن SignalR وبتتعرض جوه الشات، فمنعرضش notification.
  // (Chrome بيسمح بتخطّي الـ notification بس في الحالة دي: لازم يكون فيه window ظاهر فعلاً.)
  const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
  if (windows.some((client) => client.visibilityState === 'visible')) return;

  const tag = data.tag || `chat-${data.chatId}`;

  // تجميع: لو فيه notification لنفس الشات لسه ظاهرة، نضيف الرسالة الجديدة عليها بدل ما نعمل واحدة جديدة كل مرة
  const existing = await self.registration.getNotifications({ tag });
  const previous = existing[0]?.data;
  const lines = [...(previous?.lines ?? []), data.body].slice(-MAX_BODY_LINES);
  const count = (previous?.count ?? 0) + 1;

  await self.registration.showNotification(count > 1 ? `${data.title} • ${count} رسائل` : data.title, {
    body: lines.join('\n'),
    icon: data.icon || DEFAULT_ICON,
    tag,
    renotify: true, // يرن/يهتز تاني حتى لو نفس الـ tag
    lang: 'ar',
    dir: 'rtl',
    timestamp: data.sentAtUtc ? Date.parse(data.sentAtUtc) : Date.now(),
    data: { chatId: data.chatId, messageId: data.messageId, count, lines },
  });
}

function parsePayload(event) {
  const fallback = { title: 'رسالة جديدة', body: '' };
  if (!event.data) return fallback;

  try {
    return { ...fallback, ...event.data.json() };
  } catch {
    return { ...fallback, body: event.data.text() };
  }
}

// ─────────────────────────── Click ───────────────────────────

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const chatId = event.notification.data?.chatId;
  event.waitUntil(openChat(chatId));
});

async function openChat(chatId) {
  const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });

  // التطبيق مفتوح (حتى لو في تاب تاني): نركّز عليه ونقوله يفتح الشات من غير reload
  const target = windows.find((client) => client.visibilityState === 'visible') ?? windows[0];
  if (target) {
    try {
      await target.focus();
    } catch {
      /* بعض المتصفحات بترفض focus، مش مشكلة */
    }
    target.postMessage({ type: OPEN_CHAT_MESSAGE, chatId });
    return;
  }

  // التطبيق مقفول: نفتحه على الشات مباشرة (MainLayout بيقرا ?chatId=)
  const path = chatId ? `/chat?chatId=${encodeURIComponent(chatId)}` : '/chat';
  await self.clients.openWindow(new URL(path, self.location.origin).href);
}

// ─────────────────────── Subscription rotation ───────────────────────

// المتصفح ممكن يغيّر الـ subscription من نفسه. الـ SW مالوش وصول للتوكن، فبنعيد الاشتراك
// ونبلّغ أي window مفتوح يعمل sync مع السيرفر. ولو مفيش window، التطبيق بيعمل sync عند أول فتح.
self.addEventListener('pushsubscriptionchange', (event) => {
  event.waitUntil(
    (async () => {
      const key = event.oldSubscription?.options?.applicationServerKey;
      if (key) {
        try {
          await self.registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: key });
        } catch {
          /* هيتعمل resubscribe من التطبيق */
        }
      }

      const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
      windows.forEach((client) => client.postMessage({ type: SUBSCRIPTION_CHANGED_MESSAGE }));
    })(),
  );
});