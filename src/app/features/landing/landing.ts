import { Component, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class LandingPage {
  readonly scrolled = signal(false);

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 60);
  }

  readonly features = [
    {
      icon: 'bi-file-earmark-text',
      title: 'Submission & Intake',
      desc: 'Digitise and manage risk submissions from initial enquiry through completeness checks to underwriting review.',
    },
    {
      icon: 'bi-shield-check',
      title: 'Risk Assessment',
      desc: 'Capture structured risk data, attach evidence files and build comprehensive risk profiles for every submission.',
    },
    {
      icon: 'bi-graph-up-arrow',
      title: 'Rules & Scoring',
      desc: 'Automate underwriting decisions with configurable rule sets, scoring models and a referral matrix.',
    },
    {
      icon: 'bi-calculator',
      title: 'Pricing & Quotation',
      desc: 'Generate accurate quotes with loadings, discounts and tax calculations built on live pricing parameters.',
    },
    {
      icon: 'bi-file-earmark-medical',
      title: 'Policy Management',
      desc: 'Issue policies, process endorsements, handle cancellations and manage renewals from one unified desk.',
    },
    {
      icon: 'bi-patch-check',
      title: 'Compliance & QA',
      desc: 'Maintain audit-ready checklists, log authority breaches and track exceptions to meet regulatory standards.',
    },
    {
      icon: 'bi-bar-chart-line',
      title: 'Reporting & Analytics',
      desc: 'Real-time dashboards and portfolio analytics give management instant visibility into business performance.',
    },
    {
      icon: 'bi-people',
      title: 'Identity & Access',
      desc: 'Role-based access control ensures every user sees exactly what they need — nothing more, nothing less.',
    },
  ];

  readonly stats = [
    { value: '10+', label: 'Microservices' },
    { value: '8',   label: 'User Roles' },
    { value: '360°', label: 'Policy Lifecycle' },
    { value: '100%', label: 'Audit Tracked' },
  ];

  scrollTo(id: string): void {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
  }
}
