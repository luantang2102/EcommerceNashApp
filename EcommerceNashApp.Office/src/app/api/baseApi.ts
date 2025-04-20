import { BaseQueryApi, FetchArgs, fetchBaseQuery } from "@reduxjs/toolkit/query";
import { setLoading } from "../layout/uiSlice";
import { toast } from "react-toastify";
import { router } from "../routes/Routes";

const customBaseQuery = fetchBaseQuery({
  baseUrl: 'https://localhost:5001/api'
})

type ErrorResponse = | string | {title: string} | {errors: string[]}

export const baseQueryWithErrorHandling = async (args: string | FetchArgs, api: BaseQueryApi, extraOptions: object) => {
  api.dispatch(setLoading(true));
  const result = await customBaseQuery(args, api, extraOptions);
  api.dispatch(setLoading(false));
  if(result.error) {
    const originalStatus = result.error.status == "PARSING_ERROR" && result.error.originalStatus
     ? result.error.originalStatus : result.error.status

    const resData = result.error.data as ErrorResponse;

    switch (originalStatus) {
      case 400:
        if(typeof resData == 'string')
          toast.error(resData as string)
        else if('errors' in resData) {
          throw Object.values(resData.errors).flat().join(', ')
        }
        else toast.error(resData.title)
        break;
      case 401:
        if(typeof resData === 'object' && 'title' in resData)
          toast.error(resData.title) 
        break;
      case 404:
        if(typeof resData === 'object' && 'title' in resData)
          router.navigate('/not-found')
        break;
      case 500:
        if(typeof resData === 'object')
          router.navigate('/server-error', {state: {error: resData}})
        break;
      default:
        break;
    }
  }
  return result;
}