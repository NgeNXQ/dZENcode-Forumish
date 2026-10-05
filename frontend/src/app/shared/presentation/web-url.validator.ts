import { AbstractControl, ValidationErrors } from "@angular/forms";
import { isWebUrl } from "../orchestration/is-web-url";

export function webUrl(control: AbstractControl): ValidationErrors | null {
    const value: unknown = control.value;
    if (value === "" || value === null) return null;
    return typeof value === "string" && isWebUrl(value.trim()) ? null : { webUrl: true };
}
