import { CaptchaSettings } from "./captcha-settings";
import { CommentSettings } from "./comment-settings";
import { AttachmentSettings } from "./attachment-settings";

export interface AppSettings {
    apiBaseUrl: string;
    requestTimeoutMs: number;
    captcha: CaptchaSettings;
    comment: CommentSettings;
    attachment: AttachmentSettings;
}
