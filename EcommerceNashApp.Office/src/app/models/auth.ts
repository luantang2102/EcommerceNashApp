import { User } from "./user";


export interface AuthResponse {
    csrfToken: string;
    user: User;
}