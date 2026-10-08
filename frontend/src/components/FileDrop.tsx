import { useRef, useState, type DragEvent } from 'react'
import { formatBytes } from '../utils/format'
import { Icon } from './Icon'

interface FileDropProps {
  files: File[]
  onChange: (files: File[]) => void
  maxFiles: number
  maxTotalBytes: number
  onReject: (message: string) => void
}

export function FileDrop({ files, onChange, maxFiles, maxTotalBytes, onReject }: FileDropProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [dragging, setDragging] = useState(false)

  const totalBytes = files.reduce((sum, file) => sum + file.size, 0)

  const add = (incoming: FileList | File[]) => {
    const next = [...files]
    for (const file of Array.from(incoming)) {
      const duplicate = next.some((f) => f.name === file.name && f.size === file.size && f.lastModified === file.lastModified)
      if (!duplicate) next.push(file)
    }

    if (next.length > maxFiles) {
      onReject(`You can attach at most ${maxFiles} files.`)
      return
    }

    if (next.reduce((sum, file) => sum + file.size, 0) > maxTotalBytes) {
      onReject(`Attachments are limited to ${formatBytes(maxTotalBytes)} in total.`)
      return
    }

    onChange(next)
  }

  const onDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault()
    setDragging(false)
    if (event.dataTransfer.files.length > 0) add(event.dataTransfer.files)
  }

  return (
    <div className="field">
      <label>Attachments</label>
      <div
        className={`dropzone ${dragging ? 'dropzone-active' : ''}`}
        onDragOver={(event) => {
          event.preventDefault()
          setDragging(true)
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={onDrop}
        onClick={() => inputRef.current?.click()}
        role="button"
        tabIndex={0}
        onKeyDown={(event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault()
            inputRef.current?.click()
          }
        }}
      >
        <Icon name="upload" size={22} />
        <span>
          <strong>Click to browse</strong> or drag files here
        </span>
        <small>
          Up to {maxFiles} files, {formatBytes(maxTotalBytes)} total
        </small>
        <input
          ref={inputRef}
          type="file"
          multiple
          hidden
          onChange={(event) => {
            if (event.target.files) add(event.target.files)
            event.target.value = ''
          }}
        />
      </div>

      {files.length > 0 && (
        <>
          <ul className="file-list">
            {files.map((file) => (
              <li key={`${file.name}-${file.size}-${file.lastModified}`}>
                <Icon name="paperclip" size={14} />
                <span className="file-name" title={file.name}>
                  {file.name}
                </span>
                <span className="file-size">{formatBytes(file.size)}</span>
                <button
                  type="button"
                  className="icon-btn"
                  onClick={() => onChange(files.filter((f) => f !== file))}
                  aria-label={`Remove ${file.name}`}
                >
                  <Icon name="x" size={14} />
                </button>
              </li>
            ))}
          </ul>
          <p className="hint">
            {files.length} {files.length === 1 ? 'file' : 'files'} · {formatBytes(totalBytes)} of {formatBytes(maxTotalBytes)}
          </p>
        </>
      )}
    </div>
  )
}
