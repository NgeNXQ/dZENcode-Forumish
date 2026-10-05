import { AsyncPipe } from "@angular/common";
import { Component, inject, input, OnInit, output } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormControl, ReactiveFormsModule } from "@angular/forms";
import { combineLatest, startWith } from "rxjs";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { CaptchaAnswer } from "../../discussion/orchestration/captcha-answer";
import { CaptchaFacade } from "../orchestration/captcha-facade";

@Component({
    selector: "app-captcha",
    imports: [AsyncPipe, ReactiveFormsModule],
    providers: [CaptchaFacade],
    template: `
        @if (facade.state$ | async; as state) {
            <div class="captcha-panel">
                <div class="captcha-label">
                    <span class="tiny-label">Captcha</span>
                    <button
                        type="button"
                        class="text-button"
                        (click)="refresh()"
                        [disabled]="state.loading || disabled()"
                    >
                        ↻ New code
                    </button>
                </div>
                <div class="captcha-row">
                    <div class="captcha-image" aria-live="polite">
                        @if (state.challenge) {
                            <img
                                [src]="state.challenge.imageUrl"
                                alt="Verification code to transcribe"
                                [width]="settings.width"
                                [height]="settings.height"
                            />
                        } @else {
                            <span>{{
                                state.loading ? "Loading code…" : "Refresh to get a code"
                            }}</span>
                        }
                    </div>
                    <label class="captcha-input"
                        >Enter the code
                        <input
                            [formControl]="code"
                            [maxlength]="settings.codeLength"
                            autocomplete="off"
                            [readOnly]="disabled()"
                            autocapitalize="off"
                            spellcheck="false"
                            placeholder="{{ settings.codeLength }} characters"
                            aria-describedby="captcha-hint"
                        />
                    </label>
                </div>
                <p id="captcha-hint" class="field-hint">Letters are not case-sensitive.</p>
                @if (state.expired) {
                    <p class="field-error" role="status">Code expired. Request a new one.</p>
                }
                @if (state.error) {
                    <p class="field-error" role="alert">{{ state.error }}</p>
                }
            </div>
        }
    `,
})
export class CaptchaComponent implements OnInit {
    readonly facade = inject(CaptchaFacade);
    readonly settings = inject(APP_SETTINGS).captcha;
    readonly answer = output<CaptchaAnswer | null>();
    readonly disabled = input(false);
    readonly code = new FormControl("", { nonNullable: true });

    constructor() {
        combineLatest([this.facade.state$, this.code.valueChanges.pipe(startWith(""))])
            .pipe(takeUntilDestroyed())
            .subscribe(([state, code]) => {
                const valid = state.challenge && code.trim().length === this.settings.codeLength;
                this.answer.emit(
                    valid && state.challenge ? { id: state.challenge.id, code: code.trim() } : null,
                );
            });
    }

    ngOnInit(): void {
        this.facade.refresh();
    }
    refresh(): void {
        this.code.setValue("");
        this.facade.refresh();
    }
}
