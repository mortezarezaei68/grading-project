# ADR: Centralized Authentication with OAuth 2.0 and OpenID Connect (OIDC) via API Gateway

## Context

Our platform needs a secure, scalable, and centralized approach to manage user authentication and authorization across multiple microservices. To achieve this, we are adopting a solution where all authentication requests are handled by a single API Gateway that interacts with an Identity Provider (IdP) using OAuth 2.0 and OpenID Connect (OIDC). The API Gateway will serve as the entry point for all client requests, ensuring that only authenticated and authorized users can access protected resources in our microservices.

This solution needs to fulfill several requirements:

- **Centralized Authentication**: Users should authenticate once, and the API Gateway will ensure that their requests are properly authorized before reaching microservices.
- **Secure Token-Based Authentication**: OAuth 2.0 and OIDC will be used to issue access tokens for secure and stateless authentication.
- **Scalability**: The solution must scale as we add new microservices to the platform without introducing complexity in each service.
- **User Experience**: The solution must enable seamless Single Sign-On (SSO) for users across all services.

The key challenges include ensuring secure token handling, smooth integration with the API Gateway, and managing centralized access control.

## Decision

We will implement centralized authentication using OAuth 2.0 and OpenID Connect (OIDC), with the API Gateway acting as the centralized authentication point. Here’s the approach:

### API Gateway as Authentication Proxy

- **Single Entry Point**: The API Gateway will serve as the entry point for all client requests. It will handle authentication and token validation before routing requests to the appropriate microservice.
- **OAuth 2.0 Integration**: The API Gateway will integrate with the Identity Provider (IdP) to validate OAuth 2.0 access tokens. It will ensure that only valid tokens are passed through to downstream microservices.
- **OIDC for User Authentication**: The Gateway will leverage OIDC for authenticating users and obtaining basic user information (e.g., username, email, roles) via ID tokens. These ID tokens will be verified at the Gateway.
- **Token Validation and Propagation**: Upon receiving a request, the Gateway will validate the OAuth token (access and ID tokens) with the IdP. If valid, the token will be passed to downstream services, enabling secure, stateless communication between the services.
- **Token Handling**: The API Gateway will handle token expiration and refresh logic using refresh tokens. If the token is expired, the Gateway will ensure the user is re-authenticated and obtain a fresh access token.

### OAuth 2.0 Flows

- **Authorization Code Flow**: For web and mobile applications, the API Gateway will implement the OAuth 2.0 Authorization Code Flow to obtain access tokens and ID tokens from the IdP after user authentication.
- **Client Credentials Flow**: For machine-to-machine communication, the API Gateway will use the Client Credentials Flow, where the API Gateway itself authenticates and obtains an access token to interact with microservices.
- **Implicit Flow**: For lightweight clients (e.g., JavaScript apps), the implicit flow can be used to directly obtain access tokens without exchanging an authorization code.

### Identity Provider (IdP)

- **Centralized Identity Management**: The IdP will handle user authentication, including Single Sign-On (SSO), and provide OAuth 2.0 tokens (access and ID tokens) for communication between the client and the API Gateway.
- **Security**: The IdP will support multi-factor authentication (MFA) for added security, ensuring that only legitimate users can authenticate.
- **User Claims**: The ID token issued by the IdP will contain claims such as user roles, permissions, and identity information, which will be validated by the API Gateway and passed along to microservices.

### Role-Based Access Control (RBAC)

- **Authorization**: The API Gateway will decode the JWT tokens to extract user roles and permissions. Microservices will rely on the Gateway to handle authorization logic based on the roles and scopes defined in the token.
- **Service-Level Permissions**: Microservices will not need to manage authentication directly; instead, they will rely on the API Gateway to ensure that only authorized users can access specific resources or perform certain actions based on their roles.

### Security Considerations

- **Token Expiration**: Access tokens will be short-lived (e.g., 1 hour) to minimize the impact of token theft. Refresh tokens will be used to acquire new access tokens without requiring re-authentication.
- **Secure Token Storage**: Tokens will be securely stored on the client-side (e.g., in HTTP-only cookies or secure storage) to mitigate XSS and CSRF attacks.
- **Token Revocation**: The system will support token revocation, ensuring that tokens can be invalidated if a user logs out, their session is terminated, or a token is compromised.

## How This Applies to Microservices

- **Authentication Flow**: When a client sends a request to a microservice, the request will first go through the API Gateway. The Gateway will validate the OAuth token and authenticate the user. If the token is valid, the Gateway will forward the request to the microservice, along with the user’s roles and claims in the token.

- **Authorization**: Microservices will not directly handle user authentication. Instead, they will rely on the API Gateway to verify the token and pass along user-specific data (such as roles or permissions) for access control decisions.

- **Access Control in Microservices**: Each microservice will implement its own access control mechanisms (e.g., RBAC) based on the information provided by the API Gateway in the JWT token.

## Reasoning

- **Centralized Authentication**: Using the API Gateway for centralized authentication simplifies security management, reduces duplication, and ensures consistency across the platform. It avoids the need for each microservice to handle authentication independently, which can become complex and error-prone.

- **OAuth 2.0 and OIDC**: OAuth 2.0 provides a secure and scalable way to manage access tokens and authorization across microservices. OIDC extends OAuth 2.0 to support user authentication, making it a perfect fit for our system.

- **Scalability**: The solution scales easily because the API Gateway serves as the only point of authentication. New microservices can be added without needing to implement their own authentication logic.

- **Security**: OAuth 2.0 ensures that authentication is done securely, and with OIDC, user identity information is carried in the ID token. Tokens are validated at the Gateway, ensuring that services only receive valid requests.

## Consequences

### Positive

- **Centralized Authentication**: All authentication logic is handled in one place (API Gateway), simplifying security management and enforcement of access policies.
- **Seamless User Experience**: Users will have a single sign-on experience, where they only authenticate once and can access all authorized services.
- **Scalability**: The architecture scales easily with the addition of new microservices. The API Gateway can be extended to support new flows or services without impacting the entire platform.
- **Improved Security**: Tokens are managed centrally, reducing the risk of unauthorized access. Additionally, the ability to revoke tokens and use short-lived tokens ensures enhanced security.

### Negative

- **Single Point of Failure**: The API Gateway becomes a critical component in the authentication process. If it fails, users will be unable to authenticate, affecting the entire platform.
- **Increased Complexity at Gateway**: The API Gateway will handle not only routing but also token validation, user claims extraction, and authorization, which adds complexity to its design and maintenance.
- **Latency**: Every request will need to go through the API Gateway, potentially adding some latency, especially if the token validation or user claim extraction is not optimized.

## Steps

1. Select and integrate an Identity Provider (IdP) that supports OAuth 2.0 and OIDC (e.g., Auth0, Keycloak, Okta).
2. Configure the API Gateway to validate OAuth 2.0 tokens and handle user authentication and authorization.
3. Implement OAuth 2.0 flows (Authorization Code Flow, Client Credentials Flow) in the API Gateway for various client types (web, mobile, machine-to-machine).
4. Ensure that microservices rely on the API Gateway for authentication and authorization, avoiding direct user authentication within the services.
5. Implement security best practices for token handling, including secure storage, expiration management, and token revocation.
6. Set up monitoring and logging to track authentication and authorization events in the API Gateway and microservices.