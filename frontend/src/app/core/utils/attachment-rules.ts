export const MAX_ATTACHMENT_BYTES = 25 * 1024 * 1024;

export const ATTACHMENT_EXTENSIONS = [
  '.jpg', '.jpeg', '.png', '.gif', '.webp',
  '.mp4', '.mov', '.webm',
  '.mp3', '.wav', '.ogg', '.m4a', '.aac',
  '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.txt', '.csv', '.zip', '.rar', '.7z',
];

export const ATTACHMENT_ACCEPT = ATTACHMENT_EXTENSIONS.join(',');

export function validateAttachment(file: File): string | null {
  const ext = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
  if (!ATTACHMENT_EXTENSIONS.includes(ext)) return `نوع الملف ${ext || ''} مش مسموح`;
  if (file.size > MAX_ATTACHMENT_BYTES) return 'حجم الملف لازم يكون أقل من 25 ميجا';
  if (file.size === 0) return 'الملف فاضي';
  return null;
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}