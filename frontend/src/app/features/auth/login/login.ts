import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly email = signal('');
  readonly password = signal('');
  readonly errorMessage = signal('');
  readonly isLoading = this.authService.isLoading;

  async onSubmit(): Promise<void> {
    this.errorMessage.set('');

    const result = await this.authService.login(this.email(), this.password());

    if (result.success) {
      this.router.navigate(['/']);
    } else {
      this.errorMessage.set(result.errorMessage ?? 'حصل خطأ، حاول تاني');
    }
  }
}