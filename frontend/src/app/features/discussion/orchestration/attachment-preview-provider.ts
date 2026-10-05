import { AttachmentPreview } from "./attachment-preview";
import { Observable } from "rxjs";
export abstract class AttachmentPreviewProvider {
    abstract prepare(blob: Blob): Observable<AttachmentPreview>;
    abstract release(preview: AttachmentPreview): void;
}
