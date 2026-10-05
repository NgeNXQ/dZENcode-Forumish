import { AbstractControl, ValidationErrors } from "@angular/forms";

export function nonBlank(control: AbstractControl): ValidationErrors | null {
    const value: unknown = control.value;
    return typeof value === "string" && value.trim() ? null : { required: true };
}
