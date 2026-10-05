import { Challenge } from "./challenge";

export interface CaptchaState {
    loading: boolean;
    error: string | null;
    challenge: Challenge | null;
    expired: boolean;
}
