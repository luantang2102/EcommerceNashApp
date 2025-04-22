import { BaseQueryApi, FetchArgs, fetchBaseQuery } from "@reduxjs/toolkit/query";
import { setLoading } from "../layout/uiSlice";
import { toast } from "react-toastify";
import { router } from "../routes/Routes";

const customBaseQuery = fetchBaseQuery({
  baseUrl: "https://localhost:5001/api",
  credentials: "include", // Send cookies with requests
  prepareHeaders: (headers, { endpoint }) => {
    if (!endpoint.startsWith("auth/")) {
      const csrfToken = document.cookie
        .split("; ")
        .find((row) => row.startsWith("csrf="))
        ?.split("=")[1];
      if (csrfToken) {
        headers.set("X-CSRF-TOKEN", csrfToken); // Add CSRF token to headers
      }
    }
    return headers;
  },
});

type ErrorResponse = string | { title: string } | { errors: string[] };

export const baseQueryWithErrorHandling = async (
  args: string | FetchArgs,
  api: BaseQueryApi,
  extraOptions: object
) => {
  api.dispatch(setLoading(true));

  // Handle FormData bodies for non-auth endpoints
  let modifiedArgs = args;
  if (typeof args !== "string" && args.body instanceof FormData && !api.endpoint.startsWith("auth/")) {
    const csrfToken = document.cookie
      .split("; ")
      .find((row) => row.startsWith("csrf="))
      ?.split("=")[1];
    if (csrfToken) {
      const modifiedBody = new FormData();
      // Copy existing FormData entries
      for (const [key, value] of args.body.entries()) {
        modifiedBody.append(key, value);
      }
      // Add CSRF token as __RequestVerificationToken
      modifiedBody.append("__RequestVerificationToken", csrfToken);
      modifiedArgs = { ...args, body: modifiedBody };
    }
  }

  const result = await customBaseQuery(modifiedArgs, api, extraOptions);
  api.dispatch(setLoading(false));

  if (result.error) {
    const originalStatus =
      result.error.status === "PARSING_ERROR" && result.error.originalStatus
        ? result.error.originalStatus
        : result.error.status;

    const resData = result.error.data as ErrorResponse;

    switch (originalStatus) {
      case 400:
        if (typeof resData === "string") toast.error(resData as string);
        else if ("errors" in resData) {
          throw Object.values(resData.errors).flat().join(", ");
        } else toast.error(resData.title);
        break;
      case 401:
        if (typeof resData === "object" && "title" in resData)
          toast.error(resData.title);
        break;
      case 404:
        if (typeof resData === "object" && "title" in resData)
          router.navigate("/not-found");
        break;
      case 500:
        if (typeof resData === "object")
          router.navigate("/server-error", { state: { error: resData } });
        break;
      default:
        break;
    }
  }

  return result;
};