import axios, { AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'

export const api = axios.create({ baseURL: '/api' })

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (r) => r,
  (error: AxiosError) => {
    // Expired / invalid token: sign out quietly.
    if (error.response?.status === 401 && useAuthStore.getState().token && !error.config?.url?.includes('/auth/login')) {
      useAuthStore.getState().logout()
    }
    return Promise.reject(error)
  },
)

/** Extracts the Persian error message sent by the API ({ message }). */
export function errorMessage(error: unknown, fallback = 'خطایی رخ داد. لطفاً دوباره تلاش کنید.'): string {
  if (axios.isAxiosError(error)) {
    if (!error.response) return 'ارتباط با سرور برقرار نشد.'
    if (error.response.status === 429) return 'تعداد درخواست‌ها زیاد است. کمی بعد دوباره تلاش کنید.'
    const data = error.response.data as { message?: string; title?: string } | undefined
    return data?.message || data?.title || fallback
  }
  return fallback
}

export const get = <T,>(url: string, params?: object) => api.get<T>(url, { params }).then((r) => r.data)
export const post = <T,>(url: string, body?: unknown) => api.post<T>(url, body).then((r) => r.data)
export const put = <T,>(url: string, body?: unknown) => api.put<T>(url, body).then((r) => r.data)
export const del = <T,>(url: string) => api.delete<T>(url).then((r) => r.data)
