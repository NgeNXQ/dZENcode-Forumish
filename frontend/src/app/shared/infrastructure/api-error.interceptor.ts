import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError, timeout, TimeoutError } from "rxjs";
import { ApiFailure } from "../orchestration/api-failure";
import { APP_SETTINGS } from "../orchestration/app-settings.token";
import { isRecord } from "../orchestration/is-record";
import { parseValidationErrors } from "../orchestration/parse-validation-errors";
import { parseRetryAfter } from "../orchestration/parse-retry-after";

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
    next(request).pipe(
        timeout(inject(APP_SETTINGS).requestTimeoutMs),
        catchError((error: unknown) => {
            if (error instanceof TimeoutError) {
                return throwError(
                    () =>
                        new ApiFailure(
                            0,
                            "The request timed out. Check the discussion before posting again.",
                        ),
                );
            }
            if (!(error instanceof HttpErrorResponse)) return throwError(() => error);
            const body: unknown = error.error;
            const fields = isRecord(body) && error.status === 400
                ? parseValidationErrors(body['errors']) : {};
            const messages: Record<number, string> = {
                0: "Cannot connect to Forumish. Please check your connection and try again.",
                400: "Please check the submitted fields and try again.",
                403: "The verification code is incorrect or expired. Please try the new code.",
                404: "This conversation could not be found.",
                413: "The attachment is too large.",
                429: "You are sending requests too quickly. Please wait before trying again.",
                502: "Cannot connect to Forumish. Please try again shortly.",
                503: "Forumish is temporarily unavailable. Please try again shortly.",
                504: "Forumish is taking too long to respond. Please try again shortly.",
            };
            return throwError(
                () =>
                    new ApiFailure(
                        error.status,
                        messages[error.status] ?? "The request could not be completed.",
                        fields,
                        parseRetryAfter(error.headers.get("Retry-After")),
                    ),
            );
        }),
    );
