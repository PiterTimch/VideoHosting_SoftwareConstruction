export default {
  content: ["./src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    extend: {
      colors: {
        'theme-bg': 'rgb(var(--color-bg) / <alpha-value>)',
        black: 'rgb(var(--color-black) / <alpha-value>)',
        zinc: {
          50: 'rgb(var(--color-zinc-50) / <alpha-value>)',
          100: 'rgb(var(--color-zinc-100) / <alpha-value>)',
          200: 'rgb(var(--color-zinc-200) / <alpha-value>)',
          300: 'rgb(var(--color-zinc-300) / <alpha-value>)',
          400: 'rgb(var(--color-zinc-400) / <alpha-value>)',
          500: 'rgb(var(--color-zinc-500) / <alpha-value>)',
          600: 'rgb(var(--color-zinc-600) / <alpha-value>)',
          700: 'rgb(var(--color-zinc-700) / <alpha-value>)',
          800: 'rgb(var(--color-zinc-800) / <alpha-value>)',
          900: 'rgb(var(--color-zinc-900) / <alpha-value>)',
          950: 'rgb(var(--color-zinc-950) / <alpha-value>)',
        },
      },
      animation: {
        'spin-slow': 'spin 1.4s linear infinite',
        'play-pulse': 'playPulse 1.5s ease-in-out infinite',
      },
      keyframes: {
        playPulse: {
          '0%, 100%': {
            transform: 'scale(1)',
            boxShadow: '0 0 0 0 rgba(220, 38, 38, 0.7)',
          },
          '50%': {
            transform: 'scale(1.1)',
            boxShadow: '0 0 0 16px rgba(220, 38, 38, 0)',
          },
        },
      },
    },
  },
  plugins: [],
};
