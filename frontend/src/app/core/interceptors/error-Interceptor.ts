import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../index';

/**
 * ErrorInterceptor – بيمسك أخطاء الـ HTTP ويتعامل معاها:
 * - 401 Unauthorized → logout + redirect لـ login
 * - 403 Forbidden → redirect لـ login
 * - 500+ → log error
 * - Network errors → log error
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'حصل خطأ غير متوقع';

      if (error.error instanceof ErrorEvent) {
        // Client-side error (network, etc.)
        errorMessage = `خطأ في الاتصال: ${error.error.message}`;
        console.error('Client Error:', error.error.message);
      } else {
        // Server-side error
        switch (error.status) {
          case 401:
            errorMessage = 'جلسةك انتهت، سجل دخول تاني';
            authService.logout();
            router.navigate(['/login']);
            break;

          case 403:
            errorMessage = 'مش مسموحلك تعمل الكلام ده';
            break;

          case 404:
            errorMessage = 'المورد اللي بتدور عليه مش موجود';
            break;

          case 422:
            errorMessage = error.error?.errors?.[0] ?? 'البيانات غير صحيحة';
            break;

          case 500:
          case 502:
          case 503:
            errorMessage = 'مشكلة في السيرفر، حاول تاني بعد شوية';
            console.error('Server Error:', error.error);
            break;

          case 0:
            errorMessage = 'مفيش اتصال بالإنترنت';
            break;

          default:
            errorMessage = `خطأ ${error.status}: ${error.statusText}`;
            console.error(`HTTP ${error.status}:`, error.error);
        }
      }

      // نرجع error جديدة فيها الـ message المفهوم
      return throwError(() => new Error(errorMessage));
    }),
  );
};