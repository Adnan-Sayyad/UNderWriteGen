export interface Lookup { value: string; label: string; }

export const PRODUCT_LINES: Lookup[] = [
  { value: 'Life',       label: 'Life' },
  { value: 'Health',     label: 'Health' },
  { value: 'PnC',        label: 'Property & Casualty' },
  { value: 'Commercial', label: 'Commercial / SME' },
];

export const USER_ROLES: Lookup[] = [
  { value: 'Agent',       label: 'Agent / Broker' },
  { value: 'Underwriter', label: 'Underwriter' },
  { value: 'UWAssistant',    label: 'UW Assistant' },
  { value: 'PricingAnalyst', label: 'Pricing / Actuarial' },
  { value: 'Compliance',  label: 'Compliance / QA' },
  { value: 'Operations',  label: 'Operations / Policy Admin' },
  { value: 'Admin',       label: 'Administrator' },
];

export const RISK_BANDS: Lookup[] = [
  { value: 'Low',    label: 'Low Risk' },
  { value: 'Medium', label: 'Medium Risk' },
  { value: 'High',   label: 'High Risk' },
];
