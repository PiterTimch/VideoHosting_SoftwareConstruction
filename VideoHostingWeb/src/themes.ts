export interface Theme {
    id: string;
    name: string;
    colors: {
        bg: string;
        black: string;
        'zinc-50': string;
        'zinc-100': string;
        'zinc-200': string;
        'zinc-300': string;
        'zinc-400': string;
        'zinc-500': string;
        'zinc-600': string;
        'zinc-700': string;
        'zinc-800': string;
        'zinc-900': string;
        'zinc-950': string;
        'calendar-invert': string;
    };
}

export const themes: Theme[] = [
    {
        id: 'dark',
        name: 'Темна',
        colors: {
            bg: '18 18 19',
            black: '0 0 0',
            'zinc-50': '250 250 250',
            'zinc-100': '244 244 245',
            'zinc-200': '228 228 231',
            'zinc-300': '212 212 216',
            'zinc-400': '161 161 170',
            'zinc-500': '113 113 122',
            'zinc-600': '82 82 91',
            'zinc-700': '63 63 70',
            'zinc-800': '39 39 42',
            'zinc-900': '24 24 27',
            'zinc-950': '9 9 11',
            'calendar-invert': '1',
        }
    },
    {
        id: 'light',
        name: 'Світла',
        colors: {
            bg: '255 255 255',
            black: '9 9 11',
            'zinc-50': '9 9 11',
            'zinc-100': '24 24 27',
            'zinc-200': '39 39 42',
            'zinc-300': '63 63 70',
            'zinc-400': '82 82 91',
            'zinc-500': '113 113 122',
            'zinc-600': '161 161 170',
            'zinc-700': '212 212 216',
            'zinc-800': '228 228 231',
            'zinc-900': '244 244 245',
            'zinc-950': '255 255 255',
            'calendar-invert': '0',
        }
    }
];

export function getActiveTheme(): string {
    if (typeof window !== 'undefined' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'dark';
    }
    return 'light';
}

let themeTransitionTimer: ReturnType<typeof setTimeout> | null = null;

export function applyTheme(themeId: string) {
    const theme = themes.find(t => t.id === themeId) || themes[0];
    const root = document.documentElement;

    root.classList.add('theme-transitioning');

    Object.entries(theme.colors).forEach(([key, value]) => {
        root.style.setProperty(`--color-${key}`, value);
    });
    root.style.colorScheme = theme.id === 'dark' ? 'dark' : 'light';

    if (themeTransitionTimer) clearTimeout(themeTransitionTimer);
    themeTransitionTimer = setTimeout(() => {
        root.classList.remove('theme-transitioning');
    }, 300);
}

export function initThemeSystem(onThemeChange?: (newTheme: string) => void): () => void {
    try {
        localStorage.removeItem('theme');
    } catch (e) {
    }

    const current = getActiveTheme();
    applyTheme(current);

    if (typeof window !== 'undefined' && window.matchMedia) {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handleChange = (e: MediaQueryListEvent) => {
            const newTheme = e.matches ? 'dark' : 'light';
            applyTheme(newTheme);
            if (onThemeChange) onThemeChange(newTheme);
        };

        if (mediaQuery.addEventListener) {
            mediaQuery.addEventListener('change', handleChange);
            return () => mediaQuery.removeEventListener('change', handleChange);
        }
    }

    return () => {
    };
}
