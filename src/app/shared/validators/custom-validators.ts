import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function futureDateValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    if (!ctrl.value) return null;
    return new Date(ctrl.value) > new Date() ? null : { pastDate: true };
  };
}

export function dateRangeValidator(startKey: string, endKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value;
    const end   = group.get(endKey)?.value;
    if (!start || !end) return null;
    return new Date(start) < new Date(end) ? null : { invalidRange: true };
  };
}

export function positiveNumberValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    const val = Number(ctrl.value);
    return ctrl.value !== null && ctrl.value !== '' && (isNaN(val) || val <= 0)
      ? { notPositive: true }
      : null;
  };
}

/** Name must not contain any digit character. */
export function noDigitsValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    if (!ctrl.value) return null;
    return /\d/.test(ctrl.value) ? { hasDigits: true } : null;
  };
}

/**
 * Smart ContactInfo validator — mirrors backend SmartContactInfoAttribute.
 * - Empty/null → valid (field is optional).
 * - All digits (after stripping spaces / dashes / parens / +) → phone rules:
 *     must be exactly 10 digits and must not start with 0.
 * - Contains '@' → email rules: must be a valid email format.
 * - Anything else → invalid (unrecognised format).
 */
export function smartContactInfoValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    const raw: string = ctrl.value ?? '';
    const val = raw.trim();
    if (!val) return null;

    const stripped = val.replace(/[\s\-\(\)\+]/g, '');

    if (/^\d+$/.test(stripped)) {
      if (stripped.length !== 10)
        return { phoneLength: { actual: stripped.length } };
      if (stripped[0] === '0')
        return { phoneStartsZero: true };
      return null;
    }

    if (val.includes('@')) {
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
      if (!emailPattern.test(val))
        return { emailInvalid: true };
      return null;
    }

    return { contactUnrecognised: true };
  };
}

/**
 * DOB / Incorporation date validator.
 * - Empty → valid (optional field).
 * - Must not be a future date.
 * - Must be on or after 1 Jan 1900.
 */
export function validDOBValidator(): ValidatorFn {
  return (ctrl: AbstractControl): ValidationErrors | null => {
    if (!ctrl.value) return null;
    const date = new Date(ctrl.value);
    if (isNaN(date.getTime())) return { invalidDate: true };
    if (date > new Date())      return { futureDate: true };
    if (date.getFullYear() < 1900) return { beforeMinDate: true };
    return null;
  };
}

/** Extract human-readable error messages from an Angular HttpErrorResponse. */
export function extractApiErrors(err: any): string {
  const body = err?.error;
  if (!body) return 'Save failed. Please try again.';
  // ASP.NET Core ModelState format: { errors: { Field: ['msg1'] } }
  if (body.errors) {
    const msgs = (Object.values(body.errors) as string[][]).flat();
    if (msgs.length) return msgs.join(' ');
  }
  if (body.message) return body.message;
  return 'Save failed. Please try again.';
}
