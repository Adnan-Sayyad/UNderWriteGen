export type QuoteStatus = 'Draft' | 'Offered' | 'Accepted' | 'Expired';
export type ParamStatus = 'Active' | 'Inactive';

export interface PricingParam {
  paramId: string;
  productLine: string;
  factorName: string;
  factorTableJSON: Record<string, unknown>;
  effectiveFrom: string;
  effectiveTo: string;
  status: ParamStatus;
}

export interface Quote {
  quoteId: string;
  submissionId: string;
  versionNo: number;
  basePremium: number;
  loadingsJSON: Record<string, number>;
  discountsJSON: Record<string, number>;
  taxesJSON: Record<string, number>;
  totalPremium: number;
  termsJSON: {
    deductibles?: number;
    limits?: Record<string, number>;
    exclusions?: string[];
    subjectivities?: string[];
  };
  validUntil: string;
  status: QuoteStatus;
}
