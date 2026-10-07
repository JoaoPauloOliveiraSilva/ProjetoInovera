import React from 'react'
import type { CSSProperties } from 'react'

type IconProps = { size?: number; color?: string; filled?: boolean; style?: CSSProperties }
type AvatarProps = { size?: number; seed?: number }

export const BulbLogo = ({ size = 150 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 120 120" fill="none">
    <path
      d="M60 18c-19 0-33 13.5-33 31 0 11.5 5.8 19.5 12.5 26 3.6 3.5 5.5 6.2 5.5 10h30c0-3.8 1.9-6.5 5.5-10 6.7-6.5 12.5-14.5 12.5-26 0-17.5-14-31-33-31Z"
      stroke="#b2402f"
      strokeWidth="4.5"
      strokeLinejoin="round"
    />
    <path d="M48 85h24" stroke="#b2402f" strokeWidth="4.5" strokeLinecap="round" />
    <path d="M51 94h18" stroke="#b2402f" strokeWidth="4.5" strokeLinecap="round" />
    <path d="M48 40l12 14 12-14" stroke="#b2402f" strokeWidth="4.5" strokeLinejoin="round" strokeLinecap="round" />
  </svg>
)

export const X = ({ size = 22 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
    <path d="M4 4l16 16M20 4L4 20" />
  </svg>
)

export const ChevronRight = ({ size = 14 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9 6l6 6-6 6" />
  </svg>
)

export const ArrowDownRight = ({ size = 18 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="#b2402f" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M7 7l10 10M17 11v6h-6" />
  </svg>
)

export const ArrowUpRight = ({ size = 18 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="#b2402f" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M7 17L17 7M17 11V7h-4" />
  </svg>
)

export const Heart = ({ size = 18, filled = false }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill={filled ? '#1a1a1a' : 'none'} stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 20.5s-7.5-4.6-9.8-9.2C.7 8 2.4 4.5 6 4.5c2.2 0 3.6 1.2 4.5 2.6.9-1.4 2.3-2.6 4.5-2.6 3.6 0 5.3 3.5 3.8 6.8-2.3 4.6-9.8 9.2-9.8 9.2Z" />
  </svg>
)

export const Comment = ({ size = 18 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 12a8 8 0 0 1-11.6 7.1L4 20l1-4.4A8 8 0 1 1 21 12Z" />
  </svg>
)

export const Camera = ({ size = 22 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <path d="M4 8h3l2-2.5h6L17 8h3a1 1 0 0 1 1 1v9a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1Z" />
    <circle cx="12" cy="13" r="3.5" />
  </svg>
)

export const Check = ({ size = 16, color = '#fff' }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="M4 12.5l5 5L20 6.5" />
  </svg>
)

export const PaperPlane = ({ size = 18 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="#b2402f" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7Z" />
  </svg>
)

export const IconBooks = ({ size = 24 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round">
    <path d="M4 19V5a2 2 0 0 1 2-2h4v18H6a2 2 0 0 1-2-2Z" />
    <path d="M10 3h4a2 2 0 0 1 2 2v15h-6V3Z" />
    <path d="M16 5h2a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-2" />
  </svg>
)

export const IconUser = ({ size = 24 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="8" r="4" />
    <path d="M4 21c0-4 3.6-6.5 8-6.5s8 2.5 8 6.5" />
  </svg>
)

export const IconHelp = ({ size = 24 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="9" />
    <path d="M9.5 9.3a2.6 2.6 0 1 1 3.6 2.4c-.8.3-1.1.9-1.1 1.8" />
    <circle cx="12" cy="16.8" r="0.6" fill="currentColor" />
  </svg>
)

export const IconGrid = ({ size = 22 }: IconProps) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
    <rect x="4" y="4" width="6" height="6" rx="1" />
    <rect x="14" y="4" width="6" height="6" rx="1" />
    <rect x="4" y="14" width="6" height="6" rx="1" />
    <rect x="14" y="14" width="6" height="6" rx="1" />
  </svg>
)

export const Avatar = ({ size = 44, seed = 0 }: AvatarProps) => {
  const faces = ['#c9a2a0', '#9fb4c9', '#b9c99f', '#c9b59f']
  return (
    <div
      style={{
        width: size,
        height: size,
        borderRadius: '50%',
        background: `radial-gradient(circle at 35% 30%, ${faces[seed % faces.length]}, #6b6b66)`,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: '#fff',
        fontWeight: 700,
        fontSize: size * 0.36,
        flexShrink: 0,
      }}
    >
      {['JS', 'RM', 'NS', 'AO'][seed % 4]}
    </div>
  )
}
