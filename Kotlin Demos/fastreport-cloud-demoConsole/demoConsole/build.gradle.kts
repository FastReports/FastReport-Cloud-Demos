plugins {
    kotlin("jvm") version "1.9.22"
}

group = "org.example"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation("org.jetbrains.kotlin:kotlin-test")
    implementation("com.squareup.okhttp3:okhttp:4.11.0")
    implementation ("com.googlecode.json-simple:json-simple:1.1.1")
    implementation("org.json:json:20210307")
}

tasks.test {
    useJUnitPlatform()
}
kotlin {
    jvmToolchain(21)
}