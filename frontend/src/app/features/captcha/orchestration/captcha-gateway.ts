import { Challenge } from "./challenge";
import { Observable } from "rxjs";
export abstract class CaptchaGateway {
    abstract create(): Observable<Challenge>;
}
