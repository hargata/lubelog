LubeLogger is not designed to be deployed in serious enterprise applications. Authentication should be enabled for public(Internet-facing) deployments.

Only submit security vulnerabilities if protected resources can be accessed without authentication when it is required.

What we don't consider as security vulnerabilities:
- Your public-facing instance of LubeLogger without Authentication was defaced by malicious actors.
- A malicious actor has breached your server, accessed your postgres database and reversed the password hashes of LubeLogger users.
- A malicious actor has breached your server and replaced the Root User's Username and Password hashes with his own.
- Malware installed on your browser via extensions have injected malicious code(i.e.: clickjacking)

What we do consider as security vulnerabilities:
- Records data being accessed and modified by unauthenticated or unauthorized users.
- Malicious code that have found its way into the repository.
