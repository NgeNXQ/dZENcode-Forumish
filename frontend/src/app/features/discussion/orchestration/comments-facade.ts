import { CommentsState } from "./comments-state";
import { inject, Injectable } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { BehaviorSubject, catchError, EMPTY, Subject, switchMap, tap } from "rxjs";
import { failureMessage } from "../../../shared/orchestration/failure-message";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { DiscussionGateway } from "./discussion-gateway";
import { Reading } from "./reading";


@Injectable()
export class CommentsFacade {
    private readonly gateway = inject(DiscussionGateway);
    private readonly defaultPageSize = inject(APP_SETTINGS).comment.defaultPageSize;
    private readonly requests = new Subject<Reading>();
    private readonly state = new BehaviorSubject<CommentsState>({
        items: [],
        loading: true,
        error: null,
        page: 1,
        size: this.defaultPageSize,
        hasNext: false,
    });
    readonly state$ = this.state.asObservable();

    constructor() {
        this.requests
            .pipe(
                tap((reading) =>
                    this.state.next({
                        ...this.state.value,
                        page: reading.page,
                        size: reading.size,
                        loading: true,
                        error: null,
                        items: [],
                        hasNext: false,
                    }),
                ),
                switchMap((reading) =>
                    this.gateway.readComments(reading).pipe(
                        tap((page) =>
                            this.state.next({
                                ...this.state.value,
                                ...page,
                                loading: false,
                                hasNext: page.items.length === page.size,
                            }),
                        ),
                        catchError((error) => {
                            this.state.next({
                                ...this.state.value,
                                loading: false,
                                error: failureMessage(error),
                            });
                            return EMPTY;
                        }),
                    ),
                ),
                takeUntilDestroyed(),
            )
            .subscribe();
    }

    load(reading: Reading): void {
        this.requests.next(reading);
    }
}
