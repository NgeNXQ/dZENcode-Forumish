export interface CaptchaSettings {
    width: number;
    height: number;
    codeLength: number;
    expirationSeconds: number;
    idHeader: string;
    codeHeader: string;
}
