{{/*
The shared storage volume used by engine, log viewer, as well as the client process containers.
*/}}
{{- define "storageVol" -}}
{{ printf "%s-%s" .Values.fullname .Release.Namespace | trunc 58 }}-{{ .Release.Namespace | sha256sum | trunc 4 }}
{{- end -}}
