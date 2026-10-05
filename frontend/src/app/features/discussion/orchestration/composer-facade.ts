import { ComposerState } from "./composer-state";
import { CommentSubmission } from "./comment-submission";
import { inject, Injectable } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import {
    BehaviorSubject,
    catchError,
    EMPTY,
    exhaustMap,
    finalize,
    Subject,
    take,
    switchMap,
    tap,
    timeout,
} from "rxjs";
import { ApiFailure } from "../../../shared/orchestration/api-failure";
import { failureMessage } from "../../../shared/orchestration/failure-message";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { FingerprintProvider } from "../../identity/orchestration/fingerprint-provider";
import { DiscussionGateway } from "./discussion-gateway";
import { DiscussionEvents } from "./discussion-events";
import { CaptchaAnswer } from "./captcha-answer";
import { Comment } from "./comment";
import { Draft } from "./draft";


@Injectable()
export class ComposerFacade {
    private readonly gateway = inject(DiscussionGateway);
    private readonly identity = inject(FingerprintProvider);
    private readonly events = inject(DiscussionEvents);
    private readonly timeoutMs = inject(APP_SETTINGS).requestTimeoutMs;
    private readonly submissions = new Subject<CommentSubmission>();
    private readonly state = new BehaviorSubject<ComposerState>({ busy: false, error: null, fields: {} });
    private readonly created = new Subject<Comment>();
    private readonly consumed = new Subject<void>();
    readonly state$ = this.state.asObservable();
    readonly created$ = this.created.asObservable();
    readonly refreshCaptcha$ = this.consumed.asObservable();

    constructor() {
        this.submissions
            .pipe(
                exhaustMap(({ draft, answer }) => {
                    let attempted = false;
                    this.state.next({ busy: true, error: null, fields: {} });
                    return this.identity.identify().pipe(
                        take(1),
                        timeout(this.timeoutMs),
                        switchMap((fingerprint) => {
                            attempted = true;
                            return this.gateway.createComment(draft, fingerprint, answer);
                        }),
                        tap((comment) => {
                            this.events.announce(comment);
                            this.created.next(comment);
                        }),
                        catchError((error) => {
                            const wait =
                                error instanceof ApiFailure && error.retryAfterSeconds !== null
                                    ? ` Try again in ${Math.ceil(error.retryAfterSeconds)} seconds.`
                                    : "";
                            const message = attempted
                                ? failureMessage(error)
                                : "Browser identification could not finish. Please try again.";
                            this.state.next({
                                busy: true,
                                error: message + wait,
                                fields: error instanceof ApiFailure ? error.fields : {},
                            });
                            return EMPTY;
                        }),
                        finalize(() => {
                            this.state.next({ ...this.state.value, busy: false });
                            if (attempted) this.consumed.next();
                        }),
                    );
                }),
                takeUntilDestroyed(),
            )
            .subscribe();
    }

    post(draft: Draft, answer: CaptchaAnswer): void {
        this.submissions.next({ draft, answer });
    }
}
