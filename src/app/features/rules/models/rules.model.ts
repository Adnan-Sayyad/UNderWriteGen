// Aligned with RulesScoringAndReferralMatrix backend DTOs.
// Backend enums are serialised as integers by System.Text.Json (default).
// The API service normalises numeric enums into the string unions below.

export type RuleSeverity   = 'Block' | 'Refer' | 'Load' | 'Info';
export type UWStatus       = 'Active' | 'Inactive';
export type RiskBand       = 'Low' | 'Medium' | 'High';
export type Authority      = 'UW1' | 'UW2' | 'UWManager' | 'Committee';
export type ReferralStatus = 'Pending' | 'Approved' | 'Rejected';
export type CriteriaType   = 'SumInsured' | 'Class' | 'RiskBand';

export type ConditionOperator =
  | 'eq' | 'ne' | 'gt' | 'gte' | 'lt' | 'lte' | 'in' | 'between';

export type FieldType = 'number' | 'string' | 'boolean' | 'enum';

export const SEVERITIES:  RuleSeverity[]   = ['Block', 'Refer', 'Load', 'Info'];
export const UW_STATUSES: UWStatus[]       = ['Active', 'Inactive'];
export const RISK_BANDS:  RiskBand[]       = ['Low', 'Medium', 'High'];
export const AUTHORITIES: Authority[]      = ['UW1', 'UW2', 'UWManager', 'Committee'];
export const REFERRAL_STATUSES: ReferralStatus[] = ['Pending', 'Approved', 'Rejected'];
export const CRITERIA_TYPES: CriteriaType[] = ['SumInsured', 'Class', 'RiskBand'];

export const PRODUCT_LINES = ['Life', 'Health', 'PnC', 'Commercial'] as const;
export type ProductLine = typeof PRODUCT_LINES[number];

// ─── Operator catalog ───────────────────────────────────────────────────────
export interface OperatorOption {
  value: ConditionOperator;
  label: string;
  symbol: string;
}

export const OPERATORS_BY_TYPE: Record<FieldType, OperatorOption[]> = {
  number: [
    { value: 'eq',      label: 'equals',                  symbol: '=' },
    { value: 'ne',      label: 'not equal to',            symbol: '≠' },
    { value: 'gt',      label: 'greater than',            symbol: '>' },
    { value: 'gte',     label: 'greater than or equal',   symbol: '≥' },
    { value: 'lt',      label: 'less than',               symbol: '<' },
    { value: 'lte',     label: 'less than or equal',      symbol: '≤' },
    { value: 'between', label: 'between',                 symbol: '↔' },
  ],
  string: [
    { value: 'eq', label: 'equals',         symbol: '=' },
    { value: 'ne', label: 'not equal to',   symbol: '≠' },
    { value: 'in', label: 'is one of',      symbol: '∈' },
  ],
  boolean: [
    { value: 'eq', label: 'is',     symbol: '=' },
  ],
  enum: [
    { value: 'eq', label: 'equals',      symbol: '=' },
    { value: 'ne', label: 'not equal to', symbol: '≠' },
    { value: 'in', label: 'is one of',   symbol: '∈' },
  ],
};

// ─── Field catalog per product line ────────────────────────────────────────
export interface RuleFieldDef {
  key: string;          // technical identifier used in stored JSON
  label: string;        // friendly label shown in the picker
  type: FieldType;
  options?: string[];   // for enum / string-with-options fields
  unit?: string;        // displayed alongside numeric inputs
  description?: string; // hint shown below the field selector
}

const COMMON_FIELDS: RuleFieldDef[] = [
  { key: 'sumInsured', label: 'Sum Insured',       type: 'number', unit: '$',  description: 'Coverage amount on the policy.' },
  { key: 'riskBand',   label: 'Risk Band',         type: 'enum',   options: ['Low', 'Medium', 'High'] },
  { key: 'agentTier',  label: 'Agent Tier',        type: 'enum',   options: ['Bronze', 'Silver', 'Gold', 'Platinum'] },
  { key: 'region',     label: 'Region',            type: 'string', options: ['North', 'South', 'East', 'West', 'Central'] },
];

export const FIELDS_BY_PRODUCT: Record<ProductLine, RuleFieldDef[]> = {
  Life: [
    { key: 'age',                label: 'Age',                  type: 'number',  unit: 'yrs', description: 'Insured age at policy inception.' },
    { key: 'smoker',             label: 'Smoker',               type: 'boolean' },
    { key: 'gender',             label: 'Gender',               type: 'enum',    options: ['Male', 'Female', 'Other'] },
    { key: 'occupationHazard',   label: 'Occupation Hazard',    type: 'enum',    options: ['Low', 'Medium', 'High'] },
    { key: 'medicalConditions',  label: 'Pre-existing Conditions', type: 'boolean' },
    { key: 'familyMedicalHistory', label: 'Family Medical History', type: 'boolean' },
    ...COMMON_FIELDS,
  ],
  Health: [
    { key: 'age',                label: 'Age',                  type: 'number',  unit: 'yrs' },
    { key: 'bmi',                label: 'BMI',                  type: 'number',  description: 'Body Mass Index.' },
    { key: 'preExistingConditions', label: 'Pre-existing Conditions', type: 'boolean' },
    { key: 'chronicIllness',     label: 'Chronic Illness',      type: 'boolean' },
    { key: 'smoker',             label: 'Smoker',               type: 'boolean' },
    { key: 'dependants',         label: 'Dependants',           type: 'number' },
    ...COMMON_FIELDS,
  ],
  PnC: [
    { key: 'propertyAge',        label: 'Property Age',         type: 'number',  unit: 'yrs' },
    { key: 'propertyType',       label: 'Property Type',        type: 'enum',    options: ['Residential', 'Commercial', 'Industrial'] },
    { key: 'constructionType',   label: 'Construction Type',    type: 'enum',    options: ['Concrete', 'Steel', 'Wood', 'Mixed'] },
    { key: 'floodZone',          label: 'In Flood Zone',        type: 'boolean' },
    { key: 'earthquakeZone',     label: 'In Earthquake Zone',   type: 'boolean' },
    { key: 'claimsLast3Years',   label: 'Claims (last 3 yrs)',  type: 'number' },
    ...COMMON_FIELDS,
  ],
  Commercial: [
    { key: 'annualRevenue',      label: 'Annual Revenue',       type: 'number',  unit: '$' },
    { key: 'employeeCount',      label: 'Employee Count',       type: 'number' },
    { key: 'industryClass',      label: 'Industry Class',       type: 'enum',    options: ['Retail', 'Manufacturing', 'Services', 'Technology', 'Hospitality', 'Construction'] },
    { key: 'yearsInBusiness',    label: 'Years in Business',    type: 'number',  unit: 'yrs' },
    { key: 'priorLossRatio',     label: 'Prior Loss Ratio',     type: 'number',  unit: '%' },
    { key: 'claimsLast3Years',   label: 'Claims (last 3 yrs)',  type: 'number' },
    ...COMMON_FIELDS,
  ],
};

// ─── Stored expression shape ───────────────────────────────────────────────
export interface RuleCondition {
  field: string;
  type: FieldType;
  operator: ConditionOperator;
  value: string | number | boolean | (string | number)[] | null;
  /** Inclusive upper bound for "between" operator. */
  value2?: number | null;
  /** Friendly label snapshot so older rows still render nicely if the catalog changes. */
  fieldLabel?: string;
  unit?: string;
}

export interface RuleExpression {
  logic: 'AND' | 'OR';
  conditions: RuleCondition[];
}

export const EMPTY_EXPRESSION: RuleExpression = { logic: 'AND', conditions: [] };

// ─── Domain interfaces ────────────────────────────────────────────────────
export interface UWRule {
  uwRuleID: string;
  productLine: string;
  ruleName: string | null;
  description: string | null;
  expressionJSON: string;
  severity: RuleSeverity;
  status: UWStatus;
}

export interface RiskScore {
  riskScoreID: string;
  submissionID: string;
  modelVersion: string;
  scoreValue: number;
  band: RiskBand;
  scoredDate: string;
}

export interface ReferralMatrix {
  referralMatrixID: string;
  productLine: string;
  criteriaJSON: CriteriaType;
  operator: ConditionOperator | null;
  threshold: string | null;
  requiredAuthority: Authority;
  status: UWStatus;
}

export interface Referral {
  referralID: string;
  submissionID: string;
  raisedBy: string;
  reason: string;
  requiredAuthority: Authority;
  assignedTo: string;
  createdDate: string;
  status: ReferralStatus;
}

export interface PagedResult<T> {
  content: T[];
  page: number;
  size: number;
  totalElements: number;
  totalPages: number;
}

export const DEFAULT_PAGE_SIZE = 20;

export interface RuleEvaluationResult {
  uwRuleID: string;
  ruleName: string | null;
  productLine: string;
  severity: RuleSeverity;
  triggered: boolean;
  message: string;
}

export interface EvaluateRulesResponse {
  submissionID: string;
  results: RuleEvaluationResult[];
  hasBlockingRules: boolean;
  evaluatedAt: string;
}

// ─── Helpers ────────────────────────────────────────────────────────────────
export function parseExpression(raw: string | null | undefined): RuleExpression {
  if (!raw) return { ...EMPTY_EXPRESSION };
  try {
    const parsed = JSON.parse(raw);
    if (parsed && Array.isArray(parsed.conditions)) {
      const logic: 'AND' | 'OR' = parsed.logic === 'OR' ? 'OR' : 'AND';
      return { logic, conditions: parsed.conditions };
    }
  } catch { /* fall through */ }
  return { ...EMPTY_EXPRESSION };
}

export function operatorSymbol(op: ConditionOperator | null | undefined): string {
  const lookup: Record<ConditionOperator, string> = {
    eq: '=', ne: '≠', gt: '>', gte: '≥', lt: '<', lte: '≤',
    in: 'is one of', between: 'between',
  };
  return op ? (lookup[op] ?? op) : '';
}

export function operatorWord(op: ConditionOperator | null | undefined): string {
  const lookup: Record<ConditionOperator, string> = {
    eq: 'equals', ne: 'is not', gt: 'is greater than', gte: 'is at least',
    lt: 'is less than', lte: 'is at most', in: 'is one of', between: 'is between',
  };
  return op ? (lookup[op] ?? op) : '';
}

export function describeCondition(c: RuleCondition): string {
  const fieldLabel = c.fieldLabel ?? c.field;
  if (c.operator === 'between') {
    return `${fieldLabel} is between ${formatValue(c.value, c.unit)} and ${formatValue(c.value2 ?? null, c.unit)}`;
  }
  if (c.operator === 'in' && Array.isArray(c.value)) {
    return `${fieldLabel} is one of [${c.value.join(', ')}]`;
  }
  return `${fieldLabel} ${operatorWord(c.operator)} ${formatValue(c.value, c.unit)}`;
}

function formatValue(v: unknown, unit?: string): string {
  if (v === null || v === undefined || v === '') return '?';
  if (typeof v === 'boolean') return v ? 'true' : 'false';
  if (typeof v === 'number') {
    const formatted = unit === '$' ? `$${v.toLocaleString()}` : `${v}${unit ? ` ${unit}` : ''}`;
    return formatted;
  }
  return String(v);
}

export function describeExpression(expr: RuleExpression): string {
  if (!expr.conditions.length) return 'No conditions yet';
  if (expr.conditions.length === 1) return describeCondition(expr.conditions[0]);
  const sep = ` ${expr.logic} `;
  return expr.conditions.map(describeCondition).join(sep);
}
