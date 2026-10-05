import { inject, Injectable } from "@angular/core";
import { defer, map, Observable } from "rxjs";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { prepareAttachmentPreview } from "./prepare-attachment-preview";
import { AttachmentPreview } from "../orchestration/attachment-preview";
import { AttachmentPreviewProvider } from "../orchestration/attachment-preview-provider";

@Injectable({ providedIn: "root" })
export class BrowserAttachmentPreviewProvider extends AttachmentPreviewProvider {
    private readonly settings = inject(APP_SETTINGS).attachment;
    override prepare(blob: Blob): Observable<AttachmentPreview> {
        return defer(() => prepareAttachmentPreview(blob, this.settings)).pipe(
            map(preview => preview.text !== null ? preview : {
                ...preview, imageUrl: URL.createObjectURL(blob),
            }),
        );
    }

    override release(preview: AttachmentPreview): void {
        if (preview.imageUrl) URL.revokeObjectURL(preview.imageUrl);
    }
}
