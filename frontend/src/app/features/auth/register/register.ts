import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from 'app/core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly name = signal('');
  readonly email = signal('');
  readonly password = signal('');
  readonly errorMessage = signal('');
  readonly isLoading = this.authService.isLoading;

  async onSubmit(): Promise<void> {
    this.errorMessage.set('');

    const result = await this.authService.register(this.name(), this.email(), this.password());

    if (result.success) {
      this.router.navigate(['/']);
    } else {
      this.errorMessage.set(result.errorMessage ?? 'حصل خطأ، حاول تاني');
    }
  }
}
