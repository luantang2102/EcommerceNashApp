import { UserProfile } from "./userProfile";

export interface User {
    id: string;
    userName: string | null;
    imageUrl: string | null;
    publicId: string | null;
    email: string | null;
    roles: string[];
    createdDate: string | null;
    userProfiles: UserProfile[];
}

