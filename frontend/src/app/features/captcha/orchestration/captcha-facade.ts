import { CaptchaState } from "./captcha-state";
import { inject, Injectable } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import {
    BehaviorSubject,
    catchError,
    concat,
    EMPTY,
    map,
    of,
    Subject,
    switchMap,
    tap,
    timer,
} from "rxjs";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { failureMessage } from "../../../shared/orchestration/failure-message";
import { CaptchaGateway } from "./captcha-gateway";


@Injectable()
export class CaptchaFacade {
    private readonly gateway = inject(CaptchaGateway);
    private readonly ttl = inject(APP_SETTINGS).captcha.expirationSeconds;
    private readonly requests = new Subject<void>();
    private readonly state = new BehaviorSubject<CaptchaState>({
        loading: true,
        error: null,
        challenge: null,
        expired: false,
    });
    readonly state$ = this.state.asObservable();

    constructor() {
        this.requests
            .pipe(
                tap(() =>
                    this.state.next({
                        loading: true,
                        error: null,
                        challenge: null,
                        expired: false,
                    }),
                ),
                switchMap(() =>
                    this.gateway.create().pipe(
                        switchMap((challenge) =>
                            concat(
                                of({ loading: false, error: null, challenge, expired: false }),
                                timer(this.ttl * 1000).pipe(
                                    map(() => ({
                                        loading: false,
                                        error: null,
                                        challenge: null,
                                        expired: true,
                                    })),
                                ),
                            ),
                        ),
                        tap((state) => this.state.next(state)),
                        catchError((error) => {
                            this.state.next({
                                loading: false,
                                error: failureMessage(error),
                                challenge: null,
                                expired: false,
                            });
                            return EMPTY;
                        }),
                    ),
                ),
                takeUntilDestroyed(),
            )
            .subscribe();
    }

    refresh(): void {
        this.requests.next();
    }
}
