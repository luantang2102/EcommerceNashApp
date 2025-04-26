import { BaseQueryApi, FetchArgs, fetchBaseQuery } from "@reduxjs/toolkit/query";
import { setLoading } from "../layout/uiSlice";
import { toast } from "react-toastify";
import { router } from "../routes/Routes";
import { Mutex } from "async-mutex";

const mutex = new Mutex();

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
  // Wait for any ongoing refresh to complete
  await mutex.waitForUnlock();

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

  let result = await customBaseQuery(modifiedArgs, api, extraOptions);

  if (result.error && result.error.status === 401 && !api.endpoint.startsWith("auth/")) {
    if (!mutex.isLocked()) {
      const release = await mutex.acquire();
      try {
        // Attempt to refresh the token
        const refreshResult = await customBaseQuery(
          { url: "auth/refresh-token", method: "GET" },
          api,
          extraOptions
        );

        if (refreshResult.data) {
          result = await customBaseQuery(modifiedArgs, api, extraOptions);
        } else { 
          toast.error("Session expired. Please log in again.");
          router.navigate("/login");
        }
      } finally {
        release();
      }
    } else {
      // Another refresh is in progress, wait for it to complete
      await mutex.waitForUnlock();
      // Retry the original request
      result = await customBaseQuery(modifiedArgs, api, extraOptions);
    }
  }

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