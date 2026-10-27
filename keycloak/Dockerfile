FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /build
COPY fourprep-keycloak-extensions/pom.xml .
RUN mvn -B -q dependency:go-offline
COPY fourprep-keycloak-extensions/src ./src
RUN mvn -B -q package

FROM quay.io/keycloak/keycloak:26.8.0
COPY --from=build /build/target/fourprep-keycloak-extensions.jar /opt/keycloak/providers/
