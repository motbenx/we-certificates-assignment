import { NgClass } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css'],
  imports: [NgClass, RouterLink, RouterLinkActive],
})
export class NavMenuComponent {
  isExpanded = false;
  isReportsDropdownOpen = false;

  collapse() {
    this.isExpanded = false;
    this.isReportsDropdownOpen = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  toggleReportsDropdown() {
    this.isReportsDropdownOpen = !this.isReportsDropdownOpen;
  }

  closeReportsDropdown() {
    this.isReportsDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    const dropdown = target.closest('.dropdown');
    
    // Close dropdown if clicking outside of it
    if (!dropdown && this.isReportsDropdownOpen) {
      this.isReportsDropdownOpen = false;
    }
  }
}
