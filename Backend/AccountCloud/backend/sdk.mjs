import { createRequire } from 'node:module';
const require=createRequire(import.meta.url);
// Node checks use the original packages. Vite replaces this module with
// lazy factories so Workers never initialize SDK randomness in global scope.
export const loadAppleSdk=()=>require('@apple/app-store-server-library');
export const loadGoogleSdk=()=>require('google-auth-library');
