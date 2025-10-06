import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

interface PricingPlan {
  id: number;
  name: string;
  price?: number;
  isRecommended?: boolean;
  children?: PricingPlan[];
}

interface FlattenedPlan {
  id: number;
  fullName: string;
  price: number;
}

@Component({
  selector: 'app-pricing-plans',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pricing-plans.component.html',
  styleUrls: ['./pricing-plans.component.css']
})
export class PricingPlansComponent implements OnInit {
  pricingPlans: PricingPlan[] = [];
  filteredPlans: FlattenedPlan[] = [];
  loading = true;
  error: string | null = null;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  ngOnInit(): void {
    this.loadPricingPlans();
  }

  private loadPricingPlans(): void {
    this.http.get<PricingPlan[]>(this.baseUrl + 'PricingPlans').subscribe({
      next: (data) => {
        console.log('Received pricing plans data:', data);
        this.pricingPlans = data;
        this.processPlans();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load pricing plans';
        this.loading = false;
        console.error('Error loading pricing plans:', err);
      }
    });
  }

  private processPlans(): void {
    const flattenedPlans: FlattenedPlan[] = [];
    
    this.pricingPlans.forEach(plan => {
      this.flattenPlans(plan, '', flattenedPlans);
    });

    console.log('All flattened plans:', flattenedPlans);

    // Filter recommended plans with price between 100 and 200 (inclusive)
    this.filteredPlans = flattenedPlans
      .filter(plan => plan.price >= 100 && plan.price <= 200)
      .sort((a, b) => b.price - a.price); // Sort by price descending
    
    console.log('Filtered and sorted plans:', this.filteredPlans);
  }

  private flattenPlans(plan: PricingPlan, parentPath: string, result: FlattenedPlan[]): void {
    const currentPath = parentPath ? `${parentPath} / ${plan.name}` : plan.name;
    
    // If this is a leaf node (has price) and is recommended
    if (plan.price !== undefined && plan.isRecommended === true) {
      result.push({
        id: plan.id,
        fullName: currentPath,
        price: plan.price
      });
    }
    
    // Recursively process children
    if (plan.children) {
      plan.children.forEach(child => {
        this.flattenPlans(child, currentPath, result);
      });
    }
  }
}