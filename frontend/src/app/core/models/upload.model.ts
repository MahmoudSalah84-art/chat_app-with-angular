import { MessageType } from '../enums/message-type.enum';


export interface FileUploadResponse {
  url: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  messageType: MessageType;
}
export interface DocumentMessageContent {
  url: string;
  fileName: string;
  fileSize: number;
}


