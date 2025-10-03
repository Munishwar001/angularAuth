export interface SignupRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ApiResponse {
  success: boolean;
  message: string;
  token : string;
}


 export interface JwtPayload {
  unique_name: string;
  role: string;
  exp: number;
}
