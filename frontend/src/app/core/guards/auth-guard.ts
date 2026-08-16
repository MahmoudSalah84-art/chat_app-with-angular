import { inject, Injectable } from '@angular/core';
import { CanActivate, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard = "حارس" بيتشيك قبل ما يسمح للمستخدم يدخل صفحة معينة.
 * authGuard بيحمي الصفحات اللي محتاجة تسجيل دخول (زي صفحة الشات الرئيسية):
 * لو المستخدم مش مسجل دخول، بيحوّله لصفحة /login تلقائيًا بدل ما يشوف المحتوى.
 */

@Injectable({
  providedIn: 'root'//for injectable service
})
export class authGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }

    this.router.navigate(['/login']);
    return false;
  }
}



/**
 * guestGuard عكس authGuard: بيحمي صفحات تسجيل الدخول والتسجيل من المستخدمين
 * اللي أصلاً مسجلين دخول (عشان ميدخلوش /login تاني ويشوفوا فورم فاضي من غير داعي).
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
