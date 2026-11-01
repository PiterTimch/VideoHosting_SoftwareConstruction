import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useGetPortfolioQuery, useUpdatePortfolioMutation } from '../../services/api/apiPortfolios';
import { Button } from '../../components/form/Button';

export default function EditPortfolioPage() {
    const { authorId } = useParams<{ authorId: string }>();
    const id = Number(authorId);
    const navigate = useNavigate();
    const { data } = useGetPortfolioQuery(id, { skip: !Number.isInteger(id) || id < 1 });
    const [update, { isLoading, error }] = useUpdatePortfolioMutation();
    const [form, setForm] = useState({ name: '', nickName: '', description: '', specialization: '', contacts: '', avatarImage: undefined as File | undefined, bannerImage: undefined as File | undefined });
    useEffect(() => { if (data) setForm(f => ({ ...f, name: data.name, nickName: data.nickName, description: data.description ?? '', specialization: data.specialization ?? '', contacts: data.contacts ?? '' })); }, [data]);
    const submit = async (e: FormEvent) => { e.preventDefault(); try { await update({ id, ...form }).unwrap(); navigate(`/portfolio/${id}`); } catch { /* show API error below */ } };
    if (!data) return <p className="p-8 text-center text-zinc-400">Завантаження портфоліо…</p>;
    return <main className="mx-auto max-w-3xl p-6 text-zinc-100"><h1 className="mb-6 text-2xl font-bold">Редагувати портфоліо</h1><form onSubmit={submit} className="space-y-4">
        {([['name','Ім’я автора'],['nickName','Нікнейм'],['specialization','Спеціалізація'],['description','Опис'],['contacts','Контакти']] as const).map(([key,label]) => <label key={key} className="block text-sm text-zinc-300">{label}<input required={key==='name'||key==='nickName'} value={form[key]} onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))} className="mt-1 w-full rounded-xl border border-zinc-700 bg-zinc-900 p-3 text-white" /></label>)}
        <label className="block text-sm text-zinc-300">Аватар<input type="file" accept="image/*" onChange={e => setForm(f => ({ ...f, avatarImage: e.target.files?.[0] }))} className="mt-1 block" /></label>
        <label className="block text-sm text-zinc-300">Банер<input type="file" accept="image/*" onChange={e => setForm(f => ({ ...f, bannerImage: e.target.files?.[0] }))} className="mt-1 block" /></label>
        {error && <p role="alert" className="text-sm text-red-300">Не вдалося зберегти зміни. Перевірте нікнейм і спробуйте ще раз.</p>}
        <Button type="submit" disabled={isLoading}>{isLoading ? 'Збереження…' : 'Зберегти'}</Button>
    </form></main>;
}
