import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  templateUrl: './new-certificate.component.html',
  imports: [ReactiveFormsModule, CommonModule],
})
export class NewCertificateComponent implements OnInit {
  form: FormGroup;
  isSubmitting = false;
  validationErrors: string[] = [];
  successMessage = '';

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.form = this.fb.group({
      customerName: ['', [Validators.required]],
      customerDateOfBirth: ['', [Validators.required]],
      insuredItem: ['', [Validators.required]],
      insuredSum: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {}

  onSubmit() {
    const { valid, value } = this.form;
    this.validationErrors = [];
    this.successMessage = '';

    if (valid) {
      this.isSubmitting = true;
      
      // Convert date to ISO format and ensure sum is a number
      const requestData = {
        ...value,
        customerDateOfBirth: new Date(value.customerDateOfBirth).toISOString().split('T')[0],
        insuredSum: parseFloat(value.insuredSum)
      };

      this.http.post(this.baseUrl + 'Certificates', requestData).subscribe({
        next: (result) => {
          this.successMessage = 'Certificate created successfully!';
          this.isSubmitting = false;
          // Navigate to certificates list after a short delay
          setTimeout(() => {
            this.router.navigateByUrl('/');
          }, 2000);
        },
        error: (error: HttpErrorResponse) => {
          this.isSubmitting = false;
          console.error('Certificate creation error:', error);
          
          if (error.status === 400) {
            // Handle custom validation errors from our backend
            if (error.error?.error) {
              // Our custom error format: {error: "message"}
              this.validationErrors = [error.error.error];
            } else if (error.error?.errors) {
              // Handle ASP.NET Core model validation errors
              this.validationErrors = [];
              const errors = error.error.errors;
              
              // Extract all validation error messages
              for (const field in errors) {
                if (errors[field] && Array.isArray(errors[field])) {
                  this.validationErrors.push(...errors[field]);
                }
              }
              
              if (this.validationErrors.length === 0) {
                this.validationErrors = ['Validation failed. Please check your input.'];
              }
            } else if (error.error?.title) {
              // Handle validation errors with title
              this.validationErrors = [error.error.title];
            } else {
              this.validationErrors = ['Validation failed. Please check your input.'];
            }
          } else if (error.error?.message) {
            this.validationErrors = [error.error.message];
          } else {
            this.validationErrors = ['An unexpected error occurred. Please try again.'];
          }
        }
      });
    }
  }
}
