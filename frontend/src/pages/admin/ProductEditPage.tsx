import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowRight, ChevronLeft, ChevronRight, ExternalLink, ImagePlus, Plus, Save, Trash2 } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import toast from 'react-hot-toast'
import { Link, useNavigate, useParams } from 'react-router'
import { api, del, errorMessage, get, post, put } from '@/lib/api'
import { formatNumber } from '@/lib/format'
import type { AdminBrand, AdminCategory, AdminImage, AdminProduct, AdminVariant, ProductInput, Specification } from '@/lib/types'
import { Field, PageLoader, Spinner, Toggle } from '@/components/ui'
import { PageHeader } from './AdminLayout'

const emptyVariant: AdminVariant = { colorName: '', colorHex: '', sku: '', price: 0, discountPercent: 0, stock: 0 }

/** Converts an ISO date to the value format of <input type="datetime-local"> (local time). */
const toLocalInput = (iso?: string | null) => {
  if (!iso) return ''
  const d = new Date(iso)
  return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
}

function ImagesManager({ productId, images, onChange }: { productId: number; images: AdminImage[]; onChange: (i: AdminImage[]) => void }) {
  const input = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)

  const upload = async (files: FileList | null) => {
    if (!files?.length) return
    const form = new FormData()
    Array.from(files).forEach((f) => form.append('files', f))
    setUploading(true)
    try {
      const { data } = await api.post<AdminImage[]>(`/admin/products/${productId}/images`, form)
      onChange(data)
      toast.success('تصاویر آپلود شد')
    } catch (e) {
      toast.error(errorMessage(e))
    } finally {
      setUploading(false)
      if (input.current) input.current.value = ''
    }
  }
  const remove = async (id: number) => {
    try { onChange(await del<AdminImage[]>(`/admin/products/${productId}/images/${id}`)) } catch (e) { toast.error(errorMessage(e)) }
  }
  const move = async (index: number, dir: -1 | 1) => {
    const ids = images.map((i) => i.id)
    const j = index + dir
    if (j < 0 || j >= ids.length) return
    ;[ids[index], ids[j]] = [ids[j], ids[index]]
    try { onChange(await put<AdminImage[]>(`/admin/products/${productId}/images/order`, ids)) } catch (e) { toast.error(errorMessage(e)) }
  }

  return (
    <div className="flex flex-wrap gap-3">
      {images.map((im, i) => (
        <div key={im.id} className="group relative size-32 overflow-hidden rounded-xl border border-line bg-white">
          <img src={im.url} alt="" className="size-full object-contain" />
          {i === 0 && <span className="absolute right-1 top-1 rounded bg-brand-500 px-1.5 text-[10px] text-white">تصویر اصلی</span>}
          <div className="absolute inset-x-0 bottom-0 flex justify-between bg-black/50 p-1 text-white opacity-0 transition group-hover:opacity-100">
            <button type="button" onClick={() => move(i, -1)} aria-label="جلو"><ChevronRight size={18} /></button>
            <button type="button" onClick={() => remove(im.id)} aria-label="حذف"><Trash2 size={16} /></button>
            <button type="button" onClick={() => move(i, 1)} aria-label="عقب"><ChevronLeft size={18} /></button>
          </div>
        </div>
      ))}
      <button type="button" onClick={() => input.current?.click()} disabled={uploading}
        className="flex size-32 flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed border-line text-sm text-muted hover:border-brand-400 hover:text-brand-500">
        {uploading ? <Spinner /> : <><ImagePlus size={28} /> افزودن تصویر</>}
      </button>
      <input ref={input} type="file" accept="image/png,image/jpeg,image/webp,image/gif" multiple hidden onChange={(e) => upload(e.target.files)} />
    </div>
  )
}

export default function ProductEditPage() {
  const { id } = useParams()
  const isNew = !id
  const navigate = useNavigate()
  const qc = useQueryClient()
  const { data: product, isLoading } = useQuery({ queryKey: ['admin', 'product', id], queryFn: () => get<AdminProduct>(`/admin/products/${id}`), enabled: !isNew })
  const { data: categories = [] } = useQuery({ queryKey: ['admin', 'categories'], queryFn: () => get<AdminCategory[]>('/admin/categories') })
  const { data: brands = [] } = useQuery({ queryKey: ['admin', 'brands'], queryFn: () => get<AdminBrand[]>('/admin/brands') })

  const [form, setForm] = useState<ProductInput>({
    title: '', englishTitle: '', slug: '', description: '', categoryId: 0, brandId: null, isActive: true, isAmazingOffer: false,
    offerEndsAt: null, variants: [{ ...emptyVariant }], specifications: [],
  })
  const [images, setImages] = useState<AdminImage[]>([])

  useEffect(() => {
    if (!product) return
    setForm({
      title: product.title, englishTitle: product.englishTitle ?? '', slug: product.slug, description: product.description,
      categoryId: product.categoryId, brandId: product.brandId ?? null, isActive: product.isActive, isAmazingOffer: product.isAmazingOffer,
      offerEndsAt: product.offerEndsAt ?? null, variants: product.variants, specifications: product.specifications,
    })
    setImages(product.images)
  }, [product])

  const set = <K extends keyof ProductInput>(k: K, v: ProductInput[K]) => setForm((f) => ({ ...f, [k]: v }))
  const setVariant = (i: number, patch: Partial<AdminVariant>) => set('variants', form.variants.map((v, j) => (j === i ? { ...v, ...patch } : v)))
  const setSpec = (i: number, patch: Partial<Specification>) => set('specifications', form.specifications.map((s, j) => (j === i ? { ...s, ...patch } : s)))

  const save = useMutation({
    mutationFn: () => {
      const body: ProductInput = {
        ...form,
        brandId: form.brandId || null,
        variants: form.variants.map((v) => ({ ...v, colorHex: v.colorName ? v.colorHex || '#000000' : null, colorName: v.colorName || null })),
        specifications: form.specifications.filter((s) => s.key.trim() && s.value.trim()),
      }
      return isNew ? post<AdminProduct>('/admin/products', body) : put<AdminProduct>(`/admin/products/${id}`, body)
    },
    onSuccess: (p) => {
      qc.invalidateQueries({ queryKey: ['admin'] })
      toast.success(isNew ? 'محصول ایجاد شد. حالا تصاویر را اضافه کنید.' : 'تغییرات ذخیره شد')
      if (isNew) navigate(`/admin/products/${p.id}`, { replace: true })
    },
    onError: (e) => toast.error(errorMessage(e)),
  })

  if (!isNew && isLoading) return <PageLoader />

  return (
    <form onSubmit={(e) => { e.preventDefault(); save.mutate() }} className="space-y-5 pb-20">
      <PageHeader title={isNew ? 'محصول جدید' : 'ویرایش محصول'}
        actions={<>
          <Link to="/admin/products" className="btn-ghost"><ArrowRight size={18} /> بازگشت</Link>
          {product && <Link to={`/product/${product.slug}`} target="_blank" className="btn-ghost"><ExternalLink size={16} /> مشاهده در سایت</Link>}
          <button className="btn-primary" disabled={save.isPending}>{save.isPending ? <Spinner className="size-5 text-white" /> : <Save size={18} />} ذخیره</button>
        </>} />

      <div className="grid gap-5 xl:grid-cols-[1fr_340px]">
        <div className="space-y-5">
          <section className="card grid gap-4 p-5 md:grid-cols-2">
            <h2 className="font-bold md:col-span-2">اطلاعات اصلی</h2>
            <Field label="عنوان محصول" className="md:col-span-2"><input className="input" value={form.title} onChange={(e) => set('title', e.target.value)} required /></Field>
            <Field label="عنوان انگلیسی"><input className="input text-left" dir="ltr" value={form.englishTitle ?? ''} onChange={(e) => set('englishTitle', e.target.value)} /></Field>
            <Field label="نامک (slug) — خالی = خودکار"><input className="input text-left" dir="ltr" value={form.slug ?? ''} onChange={(e) => set('slug', e.target.value)} /></Field>
            <Field label="توضیحات" className="md:col-span-2"><textarea className="input min-h-40 leading-7" value={form.description} onChange={(e) => set('description', e.target.value)} required /></Field>
          </section>

          <section className="card p-5">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-bold">تنوع‌ها، قیمت و موجودی</h2>
              <button type="button" className="btn-outline py-1.5" onClick={() => set('variants', [...form.variants, { ...emptyVariant }])}><Plus size={16} /> تنوع جدید</button>
            </div>
            <p className="mb-3 text-xs text-muted">برای محصولات بدون رنگ، نام رنگ را خالی بگذارید. قیمت‌ها به تومان است.</p>
            <div className="space-y-3">
              {form.variants.map((v, i) => (
                <div key={v.id ?? `n${i}`} className="grid grid-cols-2 items-end gap-3 rounded-xl bg-surface/70 p-3 md:grid-cols-[1.2fr_70px_1fr_1.2fr_80px_90px_auto]">
                  <Field label="نام رنگ"><input className="input" value={v.colorName ?? ''} onChange={(e) => setVariant(i, { colorName: e.target.value })} placeholder="مثلا مشکی" /></Field>
                  <Field label="رنگ"><input type="color" className="h-[42px] w-full cursor-pointer rounded-lg border border-line bg-white p-1" value={v.colorHex || '#000000'} onChange={(e) => setVariant(i, { colorHex: e.target.value })} /></Field>
                  <Field label="کد انبار (SKU)"><input className="input text-left" dir="ltr" value={v.sku ?? ''} onChange={(e) => setVariant(i, { sku: e.target.value })} /></Field>
                  <Field label="قیمت"><input className="input text-left" dir="ltr" type="number" min={1000} value={v.price || ''} onChange={(e) => setVariant(i, { price: Number(e.target.value) })} required /></Field>
                  <Field label="تخفیف ٪"><input className="input text-left" dir="ltr" type="number" min={0} max={99} value={v.discountPercent} onChange={(e) => setVariant(i, { discountPercent: Number(e.target.value) })} /></Field>
                  <Field label="موجودی"><input className="input text-left" dir="ltr" type="number" min={0} value={v.stock} onChange={(e) => setVariant(i, { stock: Number(e.target.value) })} /></Field>
                  <button type="button" disabled={form.variants.length === 1} onClick={() => set('variants', form.variants.filter((_, j) => j !== i))}
                    className="mb-1 rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600 disabled:opacity-30" aria-label="حذف"><Trash2 size={18} /></button>
                  {v.price > 0 && v.discountPercent > 0 && (
                    <div className="col-span-full text-xs text-muted">قیمت نهایی: <b className="text-ink">{formatNumber(v.price - Math.floor((v.price * v.discountPercent) / 100))}</b> تومان</div>
                  )}
                </div>
              ))}
            </div>
          </section>

          <section className="card p-5">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-bold">مشخصات فنی</h2>
              <button type="button" className="btn-outline py-1.5" onClick={() => set('specifications', [...form.specifications, { key: '', value: '' }])}><Plus size={16} /> ویژگی جدید</button>
            </div>
            <div className="space-y-2">
              {form.specifications.map((s, i) => (
                <div key={i} className="flex gap-2">
                  <input className="input w-1/3" placeholder="عنوان (مثلا حافظه داخلی)" value={s.key} onChange={(e) => setSpec(i, { key: e.target.value })} />
                  <input className="input flex-1" placeholder="مقدار" value={s.value} onChange={(e) => setSpec(i, { value: e.target.value })} />
                  <button type="button" onClick={() => set('specifications', form.specifications.filter((_, j) => j !== i))} className="rounded-lg p-2 text-muted hover:text-rose-600"><Trash2 size={18} /></button>
                </div>
              ))}
              {form.specifications.length === 0 && <p className="text-sm text-muted">هنوز ویژگی‌ای اضافه نشده است.</p>}
            </div>
          </section>

          <section className="card p-5">
            <h2 className="mb-4 font-bold">تصاویر</h2>
            {isNew ? <p className="text-sm text-muted">ابتدا محصول را ذخیره کنید، سپس می‌توانید تصاویر را آپلود کنید.</p>
              : <ImagesManager productId={Number(id)} images={images} onChange={setImages} />}
          </section>
        </div>

        <aside className="space-y-5">
          <section className="card space-y-4 p-5">
            <h2 className="font-bold">انتشار</h2>
            <Toggle label="نمایش در فروشگاه" checked={form.isActive} onChange={(v) => set('isActive', v)} />
            <Toggle label="پیشنهاد شگفت‌انگیز" checked={form.isAmazingOffer} onChange={(v) => set('isAmazingOffer', v)} />
            {form.isAmazingOffer && (
              <Field label="پایان پیشنهاد">
                <input type="datetime-local" className="input" dir="ltr" value={toLocalInput(form.offerEndsAt)}
                  onChange={(e) => set('offerEndsAt', e.target.value ? new Date(e.target.value).toISOString() : null)} />
              </Field>
            )}
          </section>
          <section className="card space-y-4 p-5">
            <h2 className="font-bold">دسته‌بندی و برند</h2>
            <Field label="دسته‌بندی">
              <select className="input" value={form.categoryId || ''} onChange={(e) => set('categoryId', Number(e.target.value))} required>
                <option value="">انتخاب کنید</option>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.parentName ? `${c.parentName} › ` : ''}{c.name}</option>)}
              </select>
            </Field>
            <Field label="برند">
              <select className="input" value={form.brandId ?? ''} onChange={(e) => set('brandId', e.target.value ? Number(e.target.value) : null)}>
                <option value="">بدون برند</option>
                {brands.map((b) => <option key={b.id} value={b.id}>{b.name} ({b.englishName})</option>)}
              </select>
            </Field>
          </section>
        </aside>
      </div>
    </form>
  )
}
