import { Draft } from "./draft";
import { CaptchaAnswer } from "./captcha-answer";

export interface CommentSubmission {
    draft: Draft;
    answer: CaptchaAnswer;
}
