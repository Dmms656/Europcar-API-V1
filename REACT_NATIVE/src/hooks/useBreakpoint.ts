import { useEffect, useState } from 'react';
import { Platform, useWindowDimensions } from 'react-native';

export const BREAKPOINTS = {
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
} as const;

export type Breakpoint = 'mobile' | 'tablet' | 'desktop';

function resolveBreakpoint(width: number): Breakpoint {
  if (width >= BREAKPOINTS.lg) return 'desktop';
  if (width >= BREAKPOINTS.md) return 'tablet';
  return 'mobile';
}

export function useBreakpoint() {
  const { width, height } = useWindowDimensions();
  const [breakpoint, setBreakpoint] = useState<Breakpoint>(() => resolveBreakpoint(width));

  useEffect(() => {
    setBreakpoint(resolveBreakpoint(width));
  }, [width]);

  const isWeb = Platform.OS === 'web';
  const isNative = !isWeb;

  return {
    width,
    height,
    breakpoint,
    isWeb,
    isNative,
    isMobile: breakpoint === 'mobile',
    isTablet: breakpoint === 'tablet',
    isDesktop: breakpoint === 'desktop',
    /** Sidebar admin/cliente en web a partir de lg */
    showWebSidebar: isWeb && width >= BREAKPOINTS.lg,
    /** Navbar pública superior en web */
    showWebNavbar: isWeb,
  };
}
