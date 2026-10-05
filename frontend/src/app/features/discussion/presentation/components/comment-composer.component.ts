import { AsyncPipe } from "@angular/common";
import { Component, ElementRef, inject, input, output, signal, viewChild } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import {
    FormBuilder,
    ReactiveFormsModule,
    Validators,
} from "@angular/forms";
import { APP_SETTINGS } from "../../../../shared/orchestration/app-settings.token";
import { webUrl } from "../../../../shared/presentation/web-url.validator";
import { nonBlank } from "../../../../shared/presentation/non-blank.validator";
import { CaptchaComponent } from "../../../captcha/presentation/captcha.component";
import { ComposerFacade } from "../../orchestration/composer-facade";
import { CaptchaAnswer } from "../../orchestration/captcha-answer";
import { Comment } from "../../orchestration/comment";

@Component({
    selector: "app-comment-composer",
    imports: [AsyncPipe, ReactiveFormsModule, CaptchaComponent],
    providers: [ComposerFacade],
    templateUrl: "./comment-composer.component.html",
})
export class CommentComposerComponent {
    readonly facade = inject(ComposerFacade);
    readonly settings = inject(APP_SETTINGS);
    private readonly builder = inject(FormBuilder);
    readonly parent = input<Comment | null>(null);
    readonly cancelReply = output<void>();
    readonly posted = output<Comment>();
    readonly submitted = signal(false);
    readonly busy = signal(false);
    readonly proof = signal<CaptchaAnswer | null>(null);
    readonly attachment = signal<File | null>(null);
    readonly fileError = signal<string | null>(null);
    readonly messageInput = viewChild<ElementRef<HTMLTextAreaElement>>("messageInput");
    readonly fileInput = viewChild<ElementRef<HTMLInputElement>>("fileInput");
    private readonly captcha = viewChild(CaptchaComponent);
    readonly accept = Object.keys(this.settings.attachment.mime).join(",");
    readonly form = this.builder.nonNullable.group({
        email: [
            "",
            [
                nonBlank,
                Validators.email,
                Validators.maxLength(this.settings.comment.emailMaximumLength),
            ],
        ],
        username: [
            "",
            [
                nonBlank,
                Validators.pattern(/^[A-Za-z0-9]+$/),
                Validators.minLength(this.settings.comment.usernameMinimumLength),
                Validators.maxLength(this.settings.comment.usernameMaximumLength),
            ],
        ],
        homePage: ["", [webUrl, Validators.maxLength(this.settings.comment.homePageMaximumLength)]],
        message: ["", [nonBlank, Validators.maxLength(this.settings.comment.messageMaximumLength)]],
    });

    constructor() {
        this.facade.state$.pipe(takeUntilDestroyed()).subscribe((state) => this.busy.set(state.busy));
        this.facade.created$.pipe(takeUntilDestroyed()).subscribe((comment) => {
            this.form.controls.message.reset("");
            this.removeFile();
            this.submitted.set(false);
            this.posted.emit(comment);
        });
        this.facade.refreshCaptcha$.pipe(takeUntilDestroyed()).subscribe(() => {
            this.proof.set(null);
            this.captcha()?.refresh();
        });
    }

    invalid(field: keyof typeof this.form.controls): boolean {
        const control = this.form.controls[field];
        return control.invalid && (control.touched || this.submitted());
    }

    chooseFile(event: Event): void {
        if (!(event.target instanceof HTMLInputElement) || this.busy()) return;
        const file = event.target.files?.[0] ?? null;
        this.fileError.set(null);
        this.attachment.set(null);
        if (!file) return;
        const type = file.type.toLowerCase();
        const limit = Object.hasOwn(this.settings.attachment.mime, type)
            ? this.settings.attachment.mime[type] : undefined;
        if (!limit || file.size === 0 || file.size > limit) {
            this.fileError.set(
                limit
                    ? `Choose a non-empty file smaller than ${this.formatBytes(limit)}.`
                    : "Choose a JPEG, PNG, GIF, or UTF-8 plain text file.",
            );
            event.target.value = "";
            return;
        }
        this.attachment.set(file);
    }

    removeFile(): void {
        this.attachment.set(null);
        this.fileError.set(null);
        const input = this.fileInput();
        if (input) input.nativeElement.value = "";
    }

    formatBytes(bytes: number): string {
        return bytes >= 1048576
            ? `${Math.round(bytes / 1048576)} MB`
            : `${Math.round(bytes / 1024)} KB`;
    }

    format(tag: "strong" | "i" | "code" | "a"): void {
        if (this.busy()) return;
        const input = this.messageInput()?.nativeElement;
        if (!input) return;
        const start = input.selectionStart;
        const end = input.selectionEnd;
        const text = this.form.controls.message.value;
        const selected = text.slice(start, end) || "your text";
        const opening = tag === "a" ? '<a href="https://example.com">' : `<${tag}>`;
        this.form.controls.message.setValue(
            text.slice(0, start) + opening + selected + `</${tag}>` + text.slice(end),
        );
        this.form.controls.message.markAsDirty();
        input.focus();
        input.setSelectionRange(start + opening.length, start + opening.length + selected.length);
    }

    submit(): void {
        if (this.busy()) return;
        this.submitted.set(true);
        this.form.markAllAsTouched();
        const answer = this.proof();
        if (this.form.invalid || this.fileError() || !answer) return;
        this.facade.post(
            {
                ...this.form.getRawValue(),
                parentId: this.parent()?.id ?? null,
                attachment: this.attachment(),
            },
            answer,
        );
    }

    focus(): void {
        const input = this.messageInput()?.nativeElement;
        input?.focus({ preventScroll: true });
    }

    refreshCaptcha(): void {
        this.proof.set(null);
        this.captcha()?.refresh();
    }

    fieldErrors(fields: Record<string, string[]>): string[] {
        return Object.values(fields).flat();
    }
}
