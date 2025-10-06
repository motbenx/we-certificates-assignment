import { Routes } from '@angular/router';
import { CertificatesComponent } from './certificates/certificates.component';
import { NewCertificateComponent } from './new-certificate/new-certificate.component';
import { ClaimsComponent } from './claims/claims.component';
import { PricingPlansComponent } from './pricing-plans/pricing-plans.component';
import { SalesReportsComponent } from './reports/sales-reports/sales-reports.component';
import { ClaimsReportsComponent } from './reports/claims-reports/claims-reports.component';

export const appRoutes: Routes = [
  { path: '', component: CertificatesComponent, pathMatch: 'full' },
  { path: 'new-certificate', component: NewCertificateComponent },
  { path: 'claims', component: ClaimsComponent },
  { path: 'pricing-plans', component: PricingPlansComponent },
  { path: 'reports/sales', component: SalesReportsComponent },
  { path: 'reports/claims', component: ClaimsReportsComponent },
];
