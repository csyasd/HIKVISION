/**
 * 设备检测：判断是否为手机端
 */
export function isMobileDevice() {
  if (typeof window === 'undefined') return false
  const ua = navigator.userAgent.toLowerCase()
  const mobileKeywords = ['android', 'iphone', 'ipad', 'ipod', 'mobile', 'windows phone', 'webos', 'blackberry']
  const isMobileUA = mobileKeywords.some(k => ua.includes(k))
  const isSmallScreen = window.innerWidth <= 768
  return isMobileUA || isSmallScreen
}
