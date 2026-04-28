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
