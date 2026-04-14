'use client'

import { useState, useEffect, useRef } from 'react'

function toDisplay(n: number): string {
  return n > 0 ? new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n) : ''
}

function fromDisplay(s: string): number {
  const digits = s.replace(/[^0-9]/g, '')
  if (!digits) return 0
  const n = parseInt(digits, 10)
  return isNaN(n) ? 0 : n
}

export interface MoneyInputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange' | 'type'> {
  value: number
  onChange: (value: number) => void
}

/**
 * A text input that displays numeric values with comma thousand separators
 * (e.g. 1,000,000) while storing and emitting a plain number.
 */
export function MoneyInput({ value, onChange, ...rest }: MoneyInputProps) {
  const [display, setDisplay] = useState(() => toDisplay(value))
  const focusedRef = useRef(false)

  // Sync display when value changes externally (e.g. form reset), but not while typing
  useEffect(() => {
    if (!focusedRef.current) {
      setDisplay(toDisplay(value))
    }
  }, [value])

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const num = fromDisplay(e.target.value)
    const formatted = num > 0
      ? new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(num)
      : ''
    setDisplay(formatted)
    onChange(num)
  }

  function handleFocus(e: React.FocusEvent<HTMLInputElement>) {
    focusedRef.current = true
    rest.onFocus?.(e)
  }

  function handleBlur(e: React.FocusEvent<HTMLInputElement>) {
    focusedRef.current = false
    setDisplay(toDisplay(value))
    rest.onBlur?.(e)
  }

  return (
    <input
      {...rest}
      type="text"
      inputMode="numeric"
      value={display}
      onChange={handleChange}
      onFocus={handleFocus}
      onBlur={handleBlur}
    />
  )
}
