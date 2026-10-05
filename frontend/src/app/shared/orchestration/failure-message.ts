import { ApiFailure } from "./api-failure";

export function failureMessage(error: unknown): string {
    return error instanceof ApiFailure ? error.message : "Something went wrong. Please try again.";
}
