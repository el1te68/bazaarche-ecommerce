import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus, Trash2, Upload } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { api, del, errorMessage, get, post, put } from '@/lib/api'
import { img, toFaDigits } from '@/lib/format'
import type { AdminBanner, AdminBrand, AdminCategory, BannerPosition } from '@/lib/types'
import { CategoryIcon, categoryIcons } from '@/components/ui/CategoryIcon'
import { Field, Modal, PageLoader, Spinner, Toggle } from '@/components/ui'
import { PageHeader } from './AdminLayout'

function useRemove(url: string, key: string, message: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => del(`${url}/${id}`),
    onSuccess: () => { toast.success(message); qc.invalidateQueries({ queryKey: ['admin', key] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
}

async function uploadFile(url: string, file: File, field = 'file') {
  const form = new FormData()
  form.append(field, file)
  return (await api.post(url, form)).data
}

// ---------------- Categories ----------------

type CategoryForm = { name: string; slug: string; icon: string; parentId: number | null; displayOrder: number; isActive: boolean }

export function CategoriesPage() {
  const qc = useQueryClient()
  const { data = [], isLoading } = useQuery({ queryKey: ['admin', 'categories'], queryFn: () => get<AdminCategory[]>('/admin/categories') })
  const [editing, setEditing] = useState<{ id?: number; form: CategoryForm } | null>(null)
  const remove = useRemove('/admin/categories', 'categories', 'دسته‌بندی حذف شد')
  const save = useMutation({
    mutationFn: () => {
      const body = { ...editing!.form, slug: editing!.form.slug || null, icon: editing!.form.icon || null }
      return editing!.id ? put(`/admin/categories/${editing!.id}`, body) : post('/admin/categories', body)
    },
    onSuccess: () => { toast.success('ذخیره شد'); setEditing(null); qc.invalidateQueries({ queryKey: ['admin', 'categories'] }); qc.invalidateQueries({ queryKey: ['categories'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (isLoading) return <PageLoader />

  const roots = data.filter((c) => !c.parentId)
  const ordered = roots.flatMap((r) => [r, ...data.filter((c) => c.parentId === r.id)])
  const f = editing?.form
  const setF = (patch: Partial<CategoryForm>) => setEditing((e) => e && { ...e, form: { ...e.form, ...patch } })

  return (
    <div>
      <PageHeader title="دسته‌بندی‌ها" actions={
        <button className="btn-primary" onClick={() => setEditing({ form: { name: '', slug: '', icon: '', parentId: null, displayOrder: 0, isActive: true } })}><Plus size={18} /> دسته جدید</button>} />
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[640px] text-sm">
          <thead className="bg-surface text-xs text-muted"><tr><th className="p-3 text-right">نام</th><th className="p-3 text-right">نامک</th><th className="p-3 text-right">محصولات</th><th className="p-3 text-right">ترتیب</th><th className="p-3 text-right">وضعیت</th><th className="p-3" /></tr></thead>
          <tbody className="divide-y divide-line">
            {ordered.map((c) => (
              <tr key={c.id} className="hover:bg-surface/50">
                <td className="p-3"><span className={`flex items-center gap-2 ${c.parentId ? 'pr-8 text-ink/80' : 'font-bold'}`}><CategoryIcon name={c.icon} size={18} className="text-muted" />{c.name}</span></td>
                <td className="p-3 text-xs text-muted" dir="ltr">{c.slug}</td>
                <td className="p-3">{toFaDigits(c.productCount)}</td>
                <td className="p-3">{toFaDigits(c.displayOrder)}</td>
                <td className="p-3"><span className={`rounded-full px-2 py-1 text-xs ${c.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>{c.isActive ? 'فعال' : 'غیرفعال'}</span></td>
                <td className="p-3"><div className="flex justify-end gap-1">
                  <button className="rounded-lg p-2 text-muted hover:bg-surface" onClick={() => setEditing({ id: c.id, form: { name: c.name, slug: c.slug, icon: c.icon ?? '', parentId: c.parentId ?? null, displayOrder: c.displayOrder, isActive: c.isActive } })}><Pencil size={17} /></button>
                  <button className="rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600" onClick={() => confirm(`«${c.name}» حذف شود؟`) && remove.mutate(c.id)}><Trash2 size={17} /></button>
                </div></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id ? 'ویرایش دسته‌بندی' : 'دسته‌بندی جدید'}>
        {f && (
          <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); save.mutate() }}>
            <Field label="نام"><input className="input" value={f.name} onChange={(e) => setF({ name: e.target.value })} required /></Field>
            <Field label="نامک (انگلیسی، اختیاری)"><input className="input text-left" dir="ltr" value={f.slug} onChange={(e) => setF({ slug: e.target.value })} /></Field>
            <Field label="دسته والد">
              <select className="input" value={f.parentId ?? ''} onChange={(e) => setF({ parentId: e.target.value ? Number(e.target.value) : null })}>
                <option value="">— دسته اصلی —</option>
                {data.filter((c) => c.id !== editing?.id).map((c) => <option key={c.id} value={c.id}>{c.parentName ? `${c.parentName} › ` : ''}{c.name}</option>)}
              </select>
            </Field>
            <div>
              <span className="label">آیکن</span>
              <div className="grid grid-cols-8 gap-2">
                {Object.keys(categoryIcons).map((name) => (
                  <button type="button" key={name} title={name} onClick={() => setF({ icon: name })}
                    className={`flex aspect-square items-center justify-center rounded-lg border ${f.icon === name ? 'border-brand-500 bg-brand-50 text-brand-500' : 'border-line'}`}>
                    <CategoryIcon name={name} size={20} />
                  </button>
                ))}
              </div>
            </div>
            <Field label="ترتیب نمایش"><input className="input" type="number" value={f.displayOrder} onChange={(e) => setF({ displayOrder: Number(e.target.value) })} /></Field>
            <Toggle label="فعال" checked={f.isActive} onChange={(v) => setF({ isActive: v })} />
            <button className="btn-primary w-full" disabled={save.isPending}>ذخیره</button>
          </form>
        )}
      </Modal>
    </div>
  )
}

// ---------------- Brands ----------------

type BrandForm = { name: string; englishName: string; slug: string }

export function BrandsPage() {
  const qc = useQueryClient()
  const { data = [], isLoading } = useQuery({ queryKey: ['admin', 'brands'], queryFn: () => get<AdminBrand[]>('/admin/brands') })
  const [editing, setEditing] = useState<{ id?: number; form: BrandForm; logo?: File } | null>(null)
  const remove = useRemove('/admin/brands', 'brands', 'برند حذف شد')
  const save = useMutation({
    mutationFn: async () => {
      const body = { ...editing!.form, slug: editing!.form.slug || null }
      let id = editing!.id
      if (id) await put(`/admin/brands/${id}`, body)
      else id = (await post<{ id: number }>('/admin/brands', body)).id
      if (editing!.logo) await uploadFile(`/admin/brands/${id}/logo`, editing!.logo)
    },
    onSuccess: () => { toast.success('ذخیره شد'); setEditing(null); qc.invalidateQueries({ queryKey: ['admin', 'brands'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (isLoading) return <PageLoader />
  const f = editing?.form

  return (
    <div>
      <PageHeader title="برندها" actions={<button className="btn-primary" onClick={() => setEditing({ form: { name: '', englishName: '', slug: '' } })}><Plus size={18} /> برند جدید</button>} />
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4">
        {data.map((b) => (
          <div key={b.id} className="card flex items-center gap-3 p-4">
            <img src={img(b.logoUrl)} alt="" className="h-12 w-20 rounded border border-line object-contain" />
            <div className="min-w-0 flex-1">
              <div className="font-bold">{b.name}</div>
              <div className="text-xs text-muted">{b.englishName} • {toFaDigits(b.productCount)} محصول</div>
            </div>
            <button className="rounded-lg p-2 text-muted hover:bg-surface" onClick={() => setEditing({ id: b.id, form: { name: b.name, englishName: b.englishName, slug: b.slug } })}><Pencil size={17} /></button>
            <button className="rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600" onClick={() => confirm(`«${b.name}» حذف شود؟`) && remove.mutate(b.id)}><Trash2 size={17} /></button>
          </div>
        ))}
      </div>
      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id ? 'ویرایش برند' : 'برند جدید'}>
        {f && (
          <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); save.mutate() }}>
            <Field label="نام فارسی"><input className="input" value={f.name} onChange={(e) => setEditing({ ...editing!, form: { ...f, name: e.target.value } })} required /></Field>
            <Field label="نام انگلیسی"><input className="input text-left" dir="ltr" value={f.englishName} onChange={(e) => setEditing({ ...editing!, form: { ...f, englishName: e.target.value } })} required /></Field>
            <Field label="نامک (اختیاری)"><input className="input text-left" dir="ltr" value={f.slug} onChange={(e) => setEditing({ ...editing!, form: { ...f, slug: e.target.value } })} /></Field>
            <Field label="لوگو (png / jpg / webp)">
              <input type="file" accept="image/*" className="input" onChange={(e) => setEditing({ ...editing!, logo: e.target.files?.[0] })} />
            </Field>
            <button className="btn-primary w-full" disabled={save.isPending}>{save.isPending ? <Spinner className="size-5 text-white" /> : 'ذخیره'}</button>
          </form>
        )}
      </Modal>
    </div>
  )
}

// ---------------- Banners ----------------

type BannerForm = { title: string; linkUrl: string; position: BannerPosition; displayOrder: number; isActive: boolean }

export function BannersPage() {
  const qc = useQueryClient()
  const { data = [], isLoading } = useQuery({ queryKey: ['admin', 'banners'], queryFn: () => get<AdminBanner[]>('/admin/banners') })
  const [editing, setEditing] = useState<{ id?: number; form: BannerForm; image?: File; preview?: string } | null>(null)
  const remove = useRemove('/admin/banners', 'banners', 'بنر حذف شد')
  const save = useMutation({
    mutationFn: async () => {
      const form = new FormData()
      Object.entries(editing!.form).forEach(([k, v]) => form.append(k, String(v)))
      if (editing!.image) form.append('image', editing!.image)
      if (editing!.id) await api.put(`/admin/banners/${editing!.id}`, form)
      else {
        if (!editing!.image) throw new Error('تصویر بنر را انتخاب کنید.')
        await api.post('/admin/banners', form)
      }
    },
    onSuccess: () => { toast.success('ذخیره شد'); setEditing(null); qc.invalidateQueries({ queryKey: ['admin', 'banners'] }); qc.invalidateQueries({ queryKey: ['home'] }) },
    onError: (e) => toast.error(e instanceof Error && !('isAxiosError' in e) ? e.message : errorMessage(e)),
  })
  if (isLoading) return <PageLoader />
  const f = editing?.form
  const setF = (patch: Partial<BannerForm>) => setEditing((e) => e && { ...e, form: { ...e.form, ...patch } })

  const group = (position: BannerPosition, title: string, ratio: string) => (
    <section className="mb-6">
      <h2 className="mb-3 font-bold">{title}</h2>
      <div className="grid gap-4 md:grid-cols-2">
        {data.filter((b) => b.position === position).map((b) => (
          <div key={b.id} className="card overflow-hidden">
            <img src={img(b.imageUrl)} alt={b.title} className={`w-full object-cover ${ratio} ${b.isActive ? '' : 'opacity-40 grayscale'}`} />
            <div className="flex items-center gap-2 p-3 text-sm">
              <div className="flex-1"><b>{b.title}</b><div className="text-xs text-muted" dir="ltr">{b.linkUrl}</div></div>
              <button className="rounded-lg p-2 text-muted hover:bg-surface" onClick={() => setEditing({ id: b.id, preview: b.imageUrl, form: { title: b.title, linkUrl: b.linkUrl ?? '', position: b.position, displayOrder: b.displayOrder, isActive: b.isActive } })}><Pencil size={17} /></button>
              <button className="rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600" onClick={() => confirm('بنر حذف شود؟') && remove.mutate(b.id)}><Trash2 size={17} /></button>
            </div>
          </div>
        ))}
      </div>
    </section>
  )

  return (
    <div>
      <PageHeader title="بنرها" subtitle="اسلایدر اصلی و بنرهای میانی صفحه اول"
        actions={<button className="btn-primary" onClick={() => setEditing({ form: { title: '', linkUrl: '', position: 'HeroSlider', displayOrder: 0, isActive: true } })}><Plus size={18} /> بنر جدید</button>} />
      {group('HeroSlider', 'اسلایدر اصلی (پیشنهاد: ۱۶۰۰×۴۸۰)', 'aspect-[10/3]')}
      {group('MiddleBanner', 'بنرهای میانی (پیشنهاد: ۸۰۰×۴۰۰)', 'aspect-[2/1]')}
      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id ? 'ویرایش بنر' : 'بنر جدید'}>
        {f && (
          <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); save.mutate() }}>
            <label className="flex cursor-pointer flex-col items-center justify-center gap-2 overflow-hidden rounded-xl border-2 border-dashed border-line p-4 text-sm text-muted hover:border-brand-400">
              {editing?.preview ? <img src={editing.preview} alt="" className="max-h-40 rounded-lg" /> : <Upload size={28} />}
              انتخاب تصویر
              <input type="file" accept="image/*" hidden onChange={(e) => {
                const file = e.target.files?.[0]
                if (file) setEditing({ ...editing!, image: file, preview: URL.createObjectURL(file) })
              }} />
            </label>
            <Field label="عنوان"><input className="input" value={f.title} onChange={(e) => setF({ title: e.target.value })} required /></Field>
            <Field label="لینک (مثلا /category/mobile)"><input className="input text-left" dir="ltr" value={f.linkUrl} onChange={(e) => setF({ linkUrl: e.target.value })} /></Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="جایگاه">
                <select className="input" value={f.position} onChange={(e) => setF({ position: e.target.value as BannerPosition })}>
                  <option value="HeroSlider">اسلایدر اصلی</option><option value="MiddleBanner">بنر میانی</option>
                </select>
              </Field>
              <Field label="ترتیب"><input className="input" type="number" value={f.displayOrder} onChange={(e) => setF({ displayOrder: Number(e.target.value) })} /></Field>
            </div>
            <Toggle label="فعال" checked={f.isActive} onChange={(v) => setF({ isActive: v })} />
            <button className="btn-primary w-full" disabled={save.isPending}>{save.isPending ? <Spinner className="size-5 text-white" /> : 'ذخیره'}</button>
          </form>
        )}
      </Modal>
    </div>
  )
}
